using RabbitMQ.Client;

namespace Odin.Messaging.RabbitMq;

/// <summary>
/// Encapsulates a single RabbitMQ channel for publishing only, which corresponds to a distinct Exchange name.
/// Automatically re-opens the Channel if it closed.
/// </summary>
internal class SingleExchangeSender(
    string exchangeName,
    IConnection connection,
    TimeSpan sendTimeout
    ) : IDisposable
{
    private static TimeSpan RedeclareExchangeInterval { get; } = TimeSpan.FromSeconds(60);

    public void Dispose()
    {
        Channel?.CloseAsync().GetAwaiter().GetResult();
    }

    private IChannel? Channel;

    private DateTimeOffset? ExchangeRedeclaredAt;


    public async Task PublishMessage(string routingKey, Dictionary<string, object> headers, string contentType, byte[] body, bool persistentDelivery, bool mandatory)
    {
        IChannel channel = await GetOpenChannel();

        BasicProperties basicProperties = new BasicProperties
        {
            DeliveryMode = persistentDelivery ? DeliveryModes.Persistent : DeliveryModes.Transient,
            ContentType = contentType,
            Headers = headers,
            MessageId = Guid.NewGuid().ToString(),
        };

        using CancellationTokenSource cts = new CancellationTokenSource(sendTimeout);

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            body: body,
            basicProperties: basicProperties,
            mandatory: mandatory,
            cancellationToken: cts.Token
        );
    }

    private CreateChannelOptions _createChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true,
        outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(256)
        );

    private async Task<IChannel> GetOpenChannel()
    {
        if (Channel is null || !Channel.IsOpen)
        {
            Channel = await connection.CreateChannelAsync(_createChannelOptions);
        }

        if (!ExchangeRedeclaredAt.HasValue || ExchangeRedeclaredAt.Value < DateTimeOffset.Now - RedeclareExchangeInterval)
        {
            // Console.WriteLine($"Redeclaring exchange {ExchangeName}...");
            await Channel.ExchangeDeclarePassiveAsync(exchangeName);
            // Console.WriteLine($"Finished redeclaring exchange {ExchangeName}...");
            if (Channel.IsOpen)
            {
                ExchangeRedeclaredAt = DateTimeOffset.Now;
            }
        }

        if (!Channel.IsOpen)
        {
            throw new Exception($"Failed to open channel for exchange {exchangeName}. Reason: {Channel.CloseReason?.ReplyText}");
        }

        return Channel;
    }
}