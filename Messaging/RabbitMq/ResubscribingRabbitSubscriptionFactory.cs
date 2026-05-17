namespace Odin.Messaging.RabbitMq;

public interface IResubscribingRabbitSubscriptionFactory
{
    public IResubscribingRabbitSubscription Create();
}

/// <summary>
/// 
/// </summary>
/// <param name="rabbitConnectionService"></param>
/// <param name="queueName"></param>
/// <param name="autoAck"></param>
/// <param name="exclusive">Forces there to be only one consumer on the queue (enforced at the broker level).</param>
/// <param name="prefetchCount"></param>
/// <param name="checkChannelPeriod"></param>
/// <param name="attemptReconnectPeriod"></param>
public class ResubscribingRabbitSubscriptionFactory(
    IRabbitConnectionService rabbitConnectionService,
    string queueName,
    bool autoAck,
    bool exclusive,
    ushort prefetchCount = 200,
    TimeSpan? checkChannelPeriod = null,
    TimeSpan? attemptReconnectPeriod = null
    ) : IResubscribingRabbitSubscriptionFactory
{
    public IResubscribingRabbitSubscription Create()
    {
        return new ResubscribingRabbitSubscription(
            rabbitConnectionService,
            queueName,
            autoAck,
            exclusive,
            prefetchCount,
            checkChannelPeriod,
            attemptReconnectPeriod
        );

    }
}