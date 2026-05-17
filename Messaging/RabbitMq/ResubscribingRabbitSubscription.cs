namespace Odin.Messaging.RabbitMq;

public class ResubscribingRabbitSubscription : IResubscribingRabbitSubscription
{
    private IRabbitConnectionService.Subscription? _subscription;

    private long _currentSubscriptionNumber = 0;

    private SemaphoreSlim _subscriptionOperationsSemaphore = new(1);

    private CancellationTokenSource _tryCreateSubscriptionCts = new();

    private bool _shouldBeConsuming = false;

    /// <summary>
    /// Raised for messages consumed, as per Subscription.
    /// </summary>
    public event Func<IRabbitConnectionService.ConsumedMessage, Task>? OnConsumed;

    /// <summary>
    /// OnFailure is triggered when the Channel is closed, and few other failure scenarios.
    /// This event is only to notify the using code that an error has occurred. The ResubscribingRabbitSubscription will continue
    /// to attempt to create a new Subscription after this is fired.
    /// </summary>
    public event Func<Exception, Task>? OnFailure;


    private TimeSpan _attemptReconnectPeriod;
    private string _queueName;
    private bool _autoAck;
    private bool _exclusive;
    private ushort _prefetchCount;
    private TimeSpan _checkChannelPeriod;

    private IRabbitConnectionService _connectionService;

    public ResubscribingRabbitSubscription(
        IRabbitConnectionService connectionService,
        string queuename,
        bool autoAck,
        bool exclusive,
        ushort prefetchCount = 200,
        TimeSpan? checkChannelPeriod = null,
        TimeSpan? attemptReconnectPeriod = null
    )
    {
        _connectionService = connectionService;
        _queueName = queuename;
        _autoAck = autoAck;
        _exclusive = exclusive;
        _prefetchCount = prefetchCount;
        _checkChannelPeriod = checkChannelPeriod ?? TimeSpan.FromSeconds(5);
        _attemptReconnectPeriod = attemptReconnectPeriod ?? TimeSpan.FromSeconds(30);

        _ = TryCreateSubscription();
    }

    private async Task HandleFailure(long subscriptionNumber, Exception exception)
    {
        await _subscriptionOperationsSemaphore.WaitAsync();

        _ = OnFailure?.Invoke(exception);

        try
        {
            if (subscriptionNumber != _currentSubscriptionNumber)
            {
                return;
            }

            if (_subscription is null)
            {
                return;
            }

            _subscription.OnConsumed -= RaiseConsumed;

            await _subscription.CloseChannel();

            _subscription = null;
        }
        finally
        {
            _subscriptionOperationsSemaphore.Release();
        }
    }

    public async Task StartConsuming()
    {
        await _subscriptionOperationsSemaphore.WaitAsync();
        try
        {
            _shouldBeConsuming = true;
            if (_subscription is not null)
            {
                await _subscription.StartConsuming();
            }
        }
        catch (Exception ex)
        {
            OnFailure?.Invoke(ex);
        }
        finally
        {
            _subscriptionOperationsSemaphore.Release();
        }
    }

    /// <summary>
    /// Stops consuming new messages, but messages already consumed can still be acked or nacked.
    /// Shutdown procedure is to first call this, then ack (or nack) all outstanding messages, then await DisposeAsync().
    /// </summary>
    public async Task StopConsuming()
    {
        await _subscriptionOperationsSemaphore.WaitAsync();
        try
        {
            _shouldBeConsuming = false;
            if (_subscription is not null)
            {
                await _subscription.StopConsuming();
            }
        }
        catch (Exception ex)
        {
            OnFailure?.Invoke(ex);
        }
        finally
        {
            _subscriptionOperationsSemaphore.Release();
        }
    }

    private async Task TryCreateSubscription()
    {
        while (!_tryCreateSubscriptionCts.Token.IsCancellationRequested)
        {
            await _subscriptionOperationsSemaphore.WaitAsync(_tryCreateSubscriptionCts.Token);

            if (_subscription is null)
            {
                try
                {
                    _subscription = await _connectionService.SubscribeToConsume(_queueName, _autoAck, _exclusive, _prefetchCount, _checkChannelPeriod);
                    _currentSubscriptionNumber++;
                    long subNumberCopy = _currentSubscriptionNumber;
                    _subscription.OnConsumed += RaiseConsumed;
                    _subscription.OnFailure += ex =>
                    {
                        _ = HandleFailure(subNumberCopy, ex);
                        return Task.CompletedTask;
                    };
                }
                catch (Exception ex)
                {
                    _ = OnFailure?.Invoke(ex);
                    continue;
                }

                if (_shouldBeConsuming)
                {
                    try
                    {
                        await _subscription.StartConsuming();
                    }
                    catch (Exception ex)
                    {
                        _ = OnFailure?.Invoke(ex);
                        try
                        {
                            await _subscription.StopConsuming();
                        }
                        catch
                        {
                        }

                        try
                        {
                            await _subscription.CloseChannel();
                        }
                        catch
                        {
                        }

                        _subscription = null;
                    }
                }
            }

            _subscriptionOperationsSemaphore.Release();

            await Task.Delay(_attemptReconnectPeriod, _tryCreateSubscriptionCts.Token);
        }
    }

    private Task RaiseConsumed(IRabbitConnectionService.ConsumedMessage message)
    {
        _ = OnConsumed?.Invoke(message);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _tryCreateSubscriptionCts.CancelAsync();
        await _subscriptionOperationsSemaphore.WaitAsync();

        try
        {
            if (_subscription is not null)
            {
                _subscription.OnConsumed -= RaiseConsumed;
                await _subscription.CloseChannel();
            }
        }
        finally
        {
            _subscriptionOperationsSemaphore.Release();
        }
    }
}