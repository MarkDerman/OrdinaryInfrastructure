using RabbitMQ.Client;

namespace Odin.Messaging.RabbitMq;

/// <summary>
/// Helper client for a single RabbitMQ Connection (which corresponds to a single TCP connection). Each RabbitMQ Connection must correspond to a singleton RabbitConnectionService.
/// </summary>
public class RabbitConnectionService : IRabbitConnectionService
{

    private ConnectionFactory _connectionFactory;

    private long _maxChannels;

    private TimeSpan _sendTimeout;

    private string _clientProvidedName;

    public RabbitConnectionService(RabbitConnectionServiceSettings settings)
    {
        settings.Validate();
        _maxChannels = settings.MaxChannels;
        _sendTimeout = TimeSpan.FromMilliseconds(settings.SendTimeoutMillis);
        _clientProvidedName = settings.ConnectionName;
        _connectionFactory = new ConnectionFactory
        {
            AutomaticRecoveryEnabled = true,
            // DispatchConsumersAsync = false,
            // ConsumerDispatchConcurrency = 0,
            HostName = settings.Host,
            Port = settings.Port,
            TopologyRecoveryEnabled = false,
            // ClientProperties = null,
            UserName = settings.Username,
            Password = settings.UserPassword,
            VirtualHost = settings.VirtualHost,
            ClientProvidedName = settings.ConnectionName,
        };
    }

    private IConnection? _connection;

    private readonly SemaphoreSlim _createConnectionSemaphore = new(1);

    private async Task<IConnection> GetConnection()
    {
        await _createConnectionSemaphore.WaitAsync();
        try
        {
            ObjectDisposedException.ThrowIf(_isDisposing, typeof(RabbitConnectionService));

            if (_connection is not null)
            {
                return _connection;
            }

            _connection = await _connectionFactory.CreateConnectionAsync();

            return _connection;
        }
        finally
        {
            _createConnectionSemaphore.Release();
        }
    }

    private readonly Dictionary<string, SingleExchangeSender> _senders = new();

    private readonly Dictionary<string, SingleQueueListener> _listeners = new();

    private readonly SemaphoreSlim _sendersSemaphore = new(1);

    private readonly SemaphoreSlim _listenersSemaphore = new(1);

    private long GetChannelsCount()
    {
        return _senders.Count + _listeners.Count;
    }


    private async Task<SingleExchangeSender> GetSingleExchangeSender(string exchangeName)
    {

        await _sendersSemaphore.WaitAsync();

        try
        {
            ObjectDisposedException.ThrowIf(_isDisposing, typeof(RabbitConnectionService));

            if (_senders.TryGetValue(exchangeName, out SingleExchangeSender? c))
            {
                return c;
            }

            long channelsCount = GetChannelsCount();

            if (channelsCount >= _maxChannels)
            {
                throw new Exception($"Will not create new SingleExchangeSender for exchange {exchangeName} as the MaxChannels limit, {_maxChannels}, has been reached.");
            }

            IConnection connection = await GetConnection();

            SingleExchangeSender sender = new SingleExchangeSender(
                exchangeName: exchangeName,
                connection: connection,
                sendTimeout: _sendTimeout);

            _senders.Add(exchangeName, sender);

            return sender;
        }
        finally
        {
            _sendersSemaphore.Release();
        }

    }

    private async Task<SingleQueueListener> AddSingleQueueListener(string queueName, TimeSpan checkChannelPeriod, bool autoAck, bool exclusive, ushort prefetchCount, TimeSpan? channelOperationsTimeout = null)
    {
        // Yes, we are locking this dictionary for the duration of the SingleQueueListener constructor, which involves several network round trips. Subscribing should not be done very often.
        await _listenersSemaphore.WaitAsync();

        try
        {
            ObjectDisposedException.ThrowIf(_isDisposing, typeof(RabbitConnectionService));

            if (_listeners.TryGetValue(queueName, out _))
            {
                throw new ApplicationException($"Listener for queue {queueName} already exists.");
            }

            if (GetChannelsCount() >= _maxChannels)
            {
                throw new ApplicationException($"Will not create new SingleQueueListener for queue {queueName} as the MaxChannels limit, {_maxChannels}, has been reached.");
            }

            IConnection connection = await GetConnection();

            SingleQueueListener listener = new SingleQueueListener(queueName, connection, checkChannelPeriod, autoAck, exclusive, prefetchCount, _clientProvidedName, channelOperationsTimeout);

            _listeners.Add(queueName, listener);

            return listener;
        }
        finally
        {
            _listenersSemaphore.Release();
        }
    }

    private async Task RemoveSingleQueueListener(string queueName)
    {

        await _listenersSemaphore.WaitAsync();
        try
        {
            if (_listeners.Remove(queueName, out SingleQueueListener? l))
            {
                try
                {
                    await l.DisposeAsync();
                }
                catch
                {

                }
            }
        }
        finally
        {
            _listenersSemaphore.Release();
        }
    }

    public async Task<IRabbitConnectionService.Subscription> SubscribeToConsume(string queueName, bool autoAck, bool exclusive = false, ushort prefetchCount = 200,
        TimeSpan? channelCheckPeriod = null, TimeSpan? channelOperationsTimeout = null)
    {
        channelCheckPeriod ??= TimeSpan.FromSeconds(60);
        SingleQueueListener? listener = null;
        try
        {
            listener = await AddSingleQueueListener(queueName, channelCheckPeriod.Value, autoAck, exclusive, prefetchCount, channelOperationsTimeout);
        }
        catch (Exception)
        {
            if (listener is not null)
            {
                await listener.DisposeAsync();
            }
            await RemoveSingleQueueListener(queueName);
            throw;
        }

        IRabbitConnectionService.Subscription subscription = new IRabbitConnectionService.Subscription
        {
            CloseChannel = () => RemoveSingleQueueListener(queueName),
            StartConsuming = () => listener.StartConsuming(),
            StopConsuming = () => listener.StopConsuming(),
        };

        listener.OnConsume += message =>
        {
            subscription.RaiseOnConsumed(message);
            return Task.CompletedTask;
        };

        listener.OnFailure += ex =>
        {
            subscription.RaiseOnFailure(ex);
            return Task.CompletedTask;
        };

        return subscription;
    }


    public async Task SendAsync(string exchangeName, string routingKey, Dictionary<string, object> headers, string contentType, byte[] body, bool persistentDelivery = true, bool mandatoryRouting = false)
    {
        SingleExchangeSender sender = await GetSingleExchangeSender(exchangeName);
        await sender.PublishMessage(routingKey, headers, contentType, body, persistentDelivery, mandatoryRouting);
    }

    private bool _isDisposing = false;

    public async ValueTask DisposeAsync()
    {
        await _sendersSemaphore.WaitAsync();
        await _listenersSemaphore.WaitAsync();
        await _createConnectionSemaphore.WaitAsync();

        _isDisposing = true;

        foreach (SingleExchangeSender sender in _senders.Values)
        {
            try
            {
                sender.Dispose();
            }
            catch
            {
            }
        }

        foreach (SingleQueueListener listener in _listeners.Values)
        {
            try
            {
                await listener.DisposeAsync();
            }
            catch
            {
            }
        }

        try
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
            }
        }
        catch
        {
        }

        _sendersSemaphore.Release();
        _listenersSemaphore.Release();
        _createConnectionSemaphore.Release();
    }


}