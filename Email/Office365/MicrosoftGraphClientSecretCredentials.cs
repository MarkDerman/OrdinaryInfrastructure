namespace Odin.Email;

/// <summary>
/// https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp#using-a-client-certificate
/// </summary>
public record MicrosoftGraphClientSecretCredentials
{
    /// <summary>
    /// OAuth2.0 Client ID
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Azure tenant ID
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// OAuth ClientSecret
    /// </summary>
    public required string ClientSecret { get; init; }
};