using System.Text;

namespace Odin.Messaging.RabbitMq;

/// <summary>
/// Manages a single RabbitMQ Connection over which can be created
/// RabbitMQ Channels (up to a configurable maximum, currently 10) as needed.
/// A separate Channel is automatically created for each distinct Queue name subscribed to,
/// and for each distinct Exchange published to.
/// </summary>
public interface IRabbitConnectionService : IAsyncDisposable
{
    /// <summary>
    /// Throws if message cannot be published (guaranteed publishing)
    /// </summary>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    /// <param name="headers"></param>
    /// <param name="contentType"></param>
    /// <param name="body"></param>
    /// <param name="persistentDelivery"></param>
    /// <param name="mandatoryRouting">If true, throws if the message cannot be delivered (cannot be routed to any queue).</param>
    /// <returns></returns>
    public Task SendAsync(string exchangeName, string routingKey, Dictionary<string, object> headers, string contentType, byte[] body,
        bool persistentDelivery = true, bool mandatoryRouting = false);

    public class ConsumedMessage
    {
        public required byte[] Body { get; init; }
        public required DateTimeOffset ConsumedAt { get; init; }

        /// <summary>
        /// Returns true if this message is not being delivered for the first time.
        /// </summary>
        public required bool IsRedelivered { get; init; }

        /// <summary>
        /// Exchange which routed this message.
        /// </summary>
        public required string Exchange { get; init; }

        /// <summary>
        /// NB property of a RabbitMQ message
        /// </summary>
        public required string RoutingKey { get; init; }

        /// <summary>
        /// Consumer (i.e. subscription) identifier. Another RabbitMQ thing.
        /// </summary>
        public required string ConsumerTag { get; init; }

        // Message properties


        /// <summary>
        /// Optional property that might have been set by the publisher of the message
        /// </summary>
        public string? CorrelationId { get; init; }

        /// <summary>
        /// The values of this dictionary are usually byte[] containing UTF8 encoded strings.
        /// </summary>
        public Dictionary<string, object> Headers { get; init; } = new();

        public Dictionary<string, string?> GetDecodedHeaders()
        {
            return Headers.ToDictionary(p => p.Key, p =>
            {
                try
                {
                    return Encoding.UTF8.GetString((byte[])p.Value);
                }
                catch
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Optional property that might have been set by the publisher of the message
        /// </summary>
        public string? MessageId { get; init; }
        /// <summary>
        /// Optional property that might have been set by the publisher of the message
        /// </summary>
        public string? ReplyTo { get; init; }
        /// <summary>
        /// Optional property that might have been set by the publisher of the message
        /// </summary>
        public string? Type { get; init; }
        /// <summary>
        /// Optional property that might have been set by the publisher of the message
        /// </summary>
        public DateTimeOffset? Timestamp { get; init; }

        public class AcknowledgementCallbacks
        {
            /// <summary>
            /// Ack - Acknowledge. If the subscription is AutoAck = false, these callbacks are non-null and calling code MUST call either Ack or Nack after receiving the message.
            /// If the subscription is AutoAck = true, these callbacks are null.
            /// </summary>
            public required Func<ValueTask> Ack { get; init; }

            /// <summary>
            /// Nack - Not acknowledge. The bool argument of this callback is whether the nacked message should be re-queued.
            /// </summary>
            public required Func<bool, ValueTask> Nack { get; init; }
        }

        public AcknowledgementCallbacks? AckNackCallbacks { get; init; }
    }

    public class ConsumerCancelledException(string message) : ApplicationException(message);

    /// <summary>
    /// Represents a RabbitMQ Consumer that has a channel to itself.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Called for each consumed message. Exceptions thrown by the given callback are ignored.
        /// </summary>
        public event Func<ConsumedMessage, Task>? OnConsumed;

        internal void RaiseOnConsumed(ConsumedMessage message)
        {
            _ = OnConsumed?.Invoke(message);
        }

        /// <summary>
        /// Called if the connection breaks. Exceptions thrown by the given callback are ignored. May be called multiple times for the same failure.
        /// </summary>
        public event Func<Exception, Task>? OnFailure;

        internal void RaiseOnFailure(Exception exception)
        {
            _ = OnFailure?.Invoke(exception);
        }

        /// <summary>
        /// Starts consuming messages. Subscribe to events (such as OnConsumed and OnFailure) before calling this to ensure you handle all messages.
        /// </summary>
        public required Func<Task> StartConsuming { get; init; }

        /// <summary>
        /// Stops consuming new messages, but leaves the channel open so that any remaining messages that have been consumed but not acked can be acked.
        /// Shutdown procedure is to first call StopConsuming, then, ack (or nack) all remaining messages, then call CloseChannel.
        /// After consumer is cancelled, periodic checks may fire OnFailure events with exceptions of type IRabbitConnectionService.ConsumerCancelledException. 
        /// </summary>
        public required Func<Task> StopConsuming { get; init; }

        /// <summary>
        /// An async callback that, when invoked, cancels the consumer and closes the channel, regardless of any consumed but un-acked messages.
        /// </summary>
        public required Func<Task> CloseChannel { get; init; }
    }

    /// <summary>
    /// Subscribes to a RabbitMQ Queue.
    /// Throws if a subscription already exists for the queueName, even if this subscription has failed. The callback to unsubscribe must be awaited, and then
    /// re-subscription may be attempted.
    /// Throws if the initial connection, channel and consumer cannot be established.
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="autoAck">Whether the channel and consumer are in automatic acknowledgement mode. If false, the channel and consumer are in manual acknowledgement mode
    /// and the subscribing code MUST call either Ack() or Nack() on the ConsumedMessage after receiving it.</param>
    /// <param name="exclusive">When true, consumer is forced to be the only consumer of the queue. This is enforced by the broker.</param>
    /// <param name="prefetchCount">When AutoAck = false, the maximum number of messages that RabbitMQ will allow to be in-flight to the consumer (i.e. not yet acknowledged).</param>
    /// <param name="channelCheckPeriod">The period with which to periodically check that the channel is still open. Default 60 seconds. If the channel is closed, calls onFailure.</param>
    /// <param name="channelOperationsTimeout">Timeout for operations such as StartAsync and StopAsync. Default 10 seconds.</param>
    public Task<Subscription> SubscribeToConsume(string queueName, bool autoAck, bool exclusive = false, ushort prefetchCount = 200, TimeSpan? channelCheckPeriod = null, TimeSpan? channelOperationsTimeout = null);
}