namespace Odin.Messaging.RabbitMq;

public record RabbitConnectionServiceSettings
{
    /// <summary>
    /// IP or host name of RabbitMQ host
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// VHost. Default is '/'
    /// </summary>
    public string VirtualHost { get; init; } = "/";

    /// <summary>
    /// Connection UserName
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Connection password
    /// </summary>
    public required string UserPassword { get; init; }

    /// <summary>
    /// Connection port. Default is 5672
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// The client name to be used for the connection.
    /// </summary>
    public required string ConnectionName { get; init; }

    /// <summary>
    /// A separate channel will be created for each exchange published to, and for each queue subscribed to.
    /// </summary>
    public long MaxChannels { get; init; } = 20;

    /// <summary>
    /// Time after which SendAsync will throw if the publish has not been confirmed.
    /// </summary>
    public long SendTimeoutMillis { get; init; } = 5000;

    /// <summary>
    /// Throws if invalid
    /// </summary>
    /// <returns></returns>
    public void Validate()
    {
        List<string> errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Host)) errors.Add("Host is not specified");
        if (string.IsNullOrWhiteSpace(VirtualHost)) errors.Add("VirtualHost is not specified");
        if (string.IsNullOrWhiteSpace(Username)) errors.Add("UserName is not specified");
        if (string.IsNullOrWhiteSpace(UserPassword)) errors.Add("UserPassword is not specified");
        if (string.IsNullOrWhiteSpace(ConnectionName)) errors.Add("ConnectionName is not specified");
        if (MaxChannels < 1)
        {
            errors.Add("MaxChannels cannot be less than 1.");
        }
        if (errors.Count > 0)
        {
            throw new Exception($"RabbitConnectionService configuration is invalid. Errors: " + string.Join(", ", errors));
        }
    }

    public string GetConnectionString()
    {
        string vhost = VirtualHost;
        if (!vhost.StartsWith('/'))
        {
            vhost = "/" + vhost;
        }

        return $"amqp://{Username}:{UserPassword}@{Host}:{Port.ToString()}{vhost}";

    }

}