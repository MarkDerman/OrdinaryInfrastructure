using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Odin.Messaging.RabbitMq;

internal class SingleQueueListener : IAsyncDisposable
{
    private readonly string _queueName;

    private readonly string _consumerTag;

    private readonly TimeSpan _checkChannelPeriod;

    private IChannel? _channel;

    private readonly IConnection _connection;

    private AsyncEventingBasicConsumer? _consumer;

    private readonly ushort _prefetchCount;

    private readonly bool _autoAck;
    private readonly bool _exclusive;

    public event Func<Exception, Task>? OnFailure;
    public event Func<IRabbitConnectionService.ConsumedMessage, Task>? OnConsume;

    private CancellationTokenSource _checkChannelCts;
    private CancellationTokenSource _checkConsumerCts;

    public SingleQueueListener(string queueName, IConnection connection, TimeSpan checkChannelPeriod, bool autoAck, bool exclusive, ushort prefetchCount, string clientName, TimeSpan? channelOperationsTimeout = null)
    {
        _queueName = queueName;
        _checkChannelPeriod = checkChannelPeriod;
        _autoAck = autoAck;
        _exclusive = exclusive;
        _connection = connection;
        _checkChannelCts = new CancellationTokenSource();
        _checkConsumerCts = new CancellationTokenSource();
        _prefetchCount = prefetchCount;

        _consumerTag = $"{clientName}-{_queueName}-{Guid.NewGuid().ToString().Substring(0, 4)}";
    }

    private CreateChannelOptions _createChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true,
        consumerDispatchConcurrency: 8
    );

    private async Task<IChannel> GetChannel()
    {
        if (_channel is null)
        {
            _channel = await _connection.CreateChannelAsync(_createChannelOptions);
            _channel.ChannelShutdownAsync += GetChannelShutdownHandler();
            await _channel.BasicQosAsync(
                prefetchSize: 0, // PrefetchSize 0 means no limit on the number of octets of data the broker will send.
                prefetchCount: _prefetchCount,
                global: true
            );

            await _channel.QueueDeclarePassiveAsync(_queueName);
        }

        if (!_channel.IsOpen)
        {
            throw new ApplicationException($"Channel is not open. CloseReason: {_channel.CloseReason?.ReplyText}");
        }

        return _channel;
    }

    private async Task<AsyncEventingBasicConsumer> GetConsumer()
    {
        if (_consumer is null)
        {
            IChannel channel = await GetChannel();
            _consumer = new AsyncEventingBasicConsumer(channel);
            _consumer.ReceivedAsync += GetMessageReceivedHandler();
        }
        return _consumer;
    }

    public async Task StartConsuming()
    {
        IChannel channel = await GetChannel();

        AsyncEventingBasicConsumer consumer = await GetConsumer();

        if (consumer.IsRunning)
        {
            return;
        }

        await channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: _autoAck,
            consumerTag: _consumerTag,
            noLocal: false,
            exclusive: _exclusive,
            arguments: null,
            consumer: consumer
        );

        _ = Task.Run(CheckChannelPeriodically);
        _ = Task.Run(CheckConsumerPeriodically);
    }

    private AsyncEventHandler<BasicDeliverEventArgs> GetMessageReceivedHandler()
    {
        return (sender, args) =>
        {
            IRabbitConnectionService.ConsumedMessage message = new IRabbitConnectionService.ConsumedMessage
            {
                Body = args.Body.ToArray(),
                ConsumedAt = DateTimeOffset.Now,
                IsRedelivered = args.Redelivered,
                Exchange = args.Exchange,
                RoutingKey = args.RoutingKey,
                ConsumerTag = args.ConsumerTag,
                CorrelationId = args.BasicProperties?.CorrelationId,
                Headers = args.BasicProperties?.Headers?.ToDictionary() ?? new Dictionary<string, object>(),
                MessageId = args.BasicProperties?.MessageId,
                ReplyTo = args.BasicProperties?.ReplyTo,
                Type = args.BasicProperties?.Type,
                Timestamp = args.BasicProperties?.Timestamp.UnixTime is null ? null : DateTimeOffset.FromUnixTimeSeconds(args.BasicProperties.Timestamp.UnixTime),
                AckNackCallbacks = _autoAck
                    ? null
                    : new IRabbitConnectionService.ConsumedMessage.AcknowledgementCallbacks
                    {
                        Ack = GetManualAckHandler(args.DeliveryTag),
                        Nack = GetManualNackHandler(args.DeliveryTag),
                    },
            };

            return OnConsume?.Invoke(message) ?? Task.CompletedTask;
        };
    }

    private void HandleMessageReceived(object? sender, BasicDeliverEventArgs args)
    {
        IRabbitConnectionService.ConsumedMessage message = new IRabbitConnectionService.ConsumedMessage
        {
            Body = args.Body.ToArray(),
            ConsumedAt = DateTimeOffset.Now,
            IsRedelivered = args.Redelivered,
            Exchange = args.Exchange,
            RoutingKey = args.RoutingKey,
            ConsumerTag = args.ConsumerTag,
            CorrelationId = args.BasicProperties?.CorrelationId,
            Headers = args.BasicProperties?.Headers?.ToDictionary() ?? new Dictionary<string, object>(),
            MessageId = args.BasicProperties?.MessageId,
            ReplyTo = args.BasicProperties?.ReplyTo,
            Type = args.BasicProperties?.Type,
            Timestamp = args.BasicProperties?.Timestamp.UnixTime is null ? null : DateTimeOffset.FromUnixTimeSeconds(args.BasicProperties.Timestamp.UnixTime),
            AckNackCallbacks = _autoAck ? null : new IRabbitConnectionService.ConsumedMessage.AcknowledgementCallbacks
            {
                Ack = GetManualAckHandler(args.DeliveryTag),
                Nack = GetManualNackHandler(args.DeliveryTag),
            },
        };

        OnConsume?.Invoke(message);
    }

    private Func<ValueTask> GetManualAckHandler(ulong deliveryTag)
    {
        return () => _channel?.BasicAckAsync(deliveryTag, false)
                     ?? throw new ApplicationException("Channel is null");
    }

    private Func<bool, ValueTask> GetManualNackHandler(ulong deliveryTag)
    {
        return shouldRequeue => _channel?.BasicNackAsync(deliveryTag, false, shouldRequeue)
                                ?? throw new ApplicationException("Channel is null");
    }

    private async Task CheckChannelPeriodically()
    {
        while (!_checkChannelCts.Token.IsCancellationRequested)
        {
            await Task.Delay(_checkChannelPeriod, _checkChannelCts.Token);
            if (_channel is null)
            {
                OnFailure?.Invoke(new ApplicationException($"A periodic check found the channel null."));
                return;
            }
            if (!_channel.IsOpen)
            {
                OnFailure?.Invoke(new ApplicationException($"A periodic check found the channel closed. Reason: " + _channel.CloseReason.ReplyText));
            }
        }
    }

    private async Task CheckConsumerPeriodically()
    {
        while (!_checkConsumerCts.Token.IsCancellationRequested)
        {
            await Task.Delay(_checkChannelPeriod, _checkConsumerCts.Token);
            if (_consumer is null)
            {
                OnFailure?.Invoke(new ApplicationException($"A periodic check found the consumer null."));
                return;
            }
            if (!_consumer.IsRunning)
            {
                OnFailure?.Invoke(new IRabbitConnectionService.ConsumerCancelledException($"A periodic check found the consumer not running. ShutdownReason: " + _consumer.ShutdownReason.ReplyText));
            }
        }
    }

    private AsyncEventHandler<ShutdownEventArgs> GetChannelShutdownHandler()
    {
        return (sender, args) =>
        {
            ApplicationException exception = new ApplicationException($"Channel shut down. Shutdown reason: {args.ReplyText} CloseReason: " + _channel?.CloseReason?.ReplyText);
            return OnFailure?.Invoke(exception) ?? Task.CompletedTask;
        };

    }


    public async Task StopConsuming()
    {
        await _checkConsumerCts.CancelAsync();

        if (!_channel.IsOpen)
        {
            throw new ApplicationException("Cannot cancel consuming since channel is not open.");
        }

        await _channel.BasicCancelAsync(_consumerTag);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopConsuming();
        }
        catch
        {
        }

        await _checkChannelCts.CancelAsync();

        try
        {
            await (_channel?.CloseAsync() ?? Task.CompletedTask);
        }
        catch
        {
        }

        _channel?.Dispose();
    }
}