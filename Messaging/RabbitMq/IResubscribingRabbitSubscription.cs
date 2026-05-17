namespace Odin.Messaging.RabbitMq;

public interface IResubscribingRabbitSubscription : IAsyncDisposable
{
    /// <summary>
    /// Raised for messages consumed. See documentation of IRabbitService.Subscription
    /// </summary>
    public event Func<IRabbitConnectionService.ConsumedMessage, Task>? OnConsumed;

    /// <summary>
    /// OnFailure is triggered when the Channel is closed, and few other failure scenarios.
    /// </summary>
    public event Func<Exception, Task>? OnFailure;

    /// <summary>
    /// Starts consuming from the queue. To ensure all messages are handled, subscribe to OnConsumed before awaiting this.
    /// </summary>
    /// <returns></returns>
    public Task StartConsuming();

    /// <summary>
    /// Stops consuming new messages, but messages already consumed can still be acked or nacked.
    /// Shutdown procedure is to first call this, then ack (or nack) all outstanding messages, then await DisposeAsync().
    /// </summary>
    public Task StopConsuming();
}