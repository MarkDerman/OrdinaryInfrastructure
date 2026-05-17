namespace Odin.Messaging.RabbitMq;

/// <summary>
/// Minimalistic implementation that does nothing.
/// </summary>
public class NullRabbitConnectionService : IRabbitConnectionService
{
    public async Task SendAsync(string exchangeName, string routingKey, Dictionary<string, object> headers, string contentType, byte[] body, bool persistentDelivery = true,
        bool mandatoryRouting = false)
    {
        await Task.Delay(5);
    }

    public async Task<IRabbitConnectionService.Subscription> SubscribeToConsume(string queueName, bool autoAck, bool exclusive = false, ushort prefetchCount = 200,
        TimeSpan? channelCheckPeriod = null, TimeSpan? channelOperationsTimeout = null)
    {
        await Task.Delay(5);

        return new IRabbitConnectionService.Subscription
        {
            StartConsuming = () => Task.CompletedTask,
            StopConsuming = () => Task.CompletedTask,
            CloseChannel = () => Task.CompletedTask,
        };
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}