namespace Odin.Configuration.Hosting;

/// <summary>
/// Controls how Azure Key Vault configuration is discovered and added to a host.
/// </summary>
/// <remarks>
/// Defaults match Soulv app settings conventions: values are read from the <c>AzureKeyVault</c>
/// section, the runtime environment comes from <c>Environment</c>, and local environments may skip vault access.
/// </remarks>
public sealed class KeyVaultConfigurationOptions
{
    /// <summary>
    /// Gets or sets the configuration section that contains Azure Key Vault settings.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>AzureKeyVault</c>. Expected child keys include <c>Name</c>, <c>Prefix</c>,
    /// <c>SkipKeyVaultConfigInjection</c>, <c>ManagedIdentityClientId</c>, and <c>ClientSecret</c>.
    /// </remarks>
    public string SectionName { get; set; } = "AzureKeyVault";

    /// <summary>
    /// Gets or sets the configuration key used to read the Azure AD tenant id.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>AzureAd:TenantId</c>. This value is required when using client secret credentials.
    /// </remarks>
    public string AzureAdTenantIdKey { get; set; } = "AzureAd:TenantId";

    /// <summary>
    /// Gets or sets the environment name used when no environment value is configured.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Development</c>. This value is used when deriving the default Key Vault secret prefix.
    /// </remarks>
    public string DefaultEnvironmentName { get; set; } = "Development";

    /// <summary>
    /// Gets or sets the configuration key used to read the host environment name.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Environment</c>. The value participates in the default Key Vault prefix:
    /// <c>{ApplicationNameLastSegment}-{Environment}-</c>.
    /// </remarks>
    public string EnvironmentKey { get; set; } = "Environment";

    /// <summary>
    /// Gets or sets the application name segment used when deriving the default Key Vault secret prefix.
    /// </summary>
    /// <remarks>
    /// When unset, the last dotted segment of the host application name is used. For example,
    /// <c>Flash.FinanceRecons</c> becomes <c>FinanceRecons</c>.
    /// </remarks>
    public string? PrefixApplicationName { get; set; }

    /// <summary>
    /// Gets or sets environment names treated as local development environments.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Development</c>, <c>Dev</c>, and <c>Local</c>. Local environments may skip
    /// Key Vault when no credential is configured, allowing local development to use files and user secrets.
    /// </remarks>
    public string[] LocalEnvironmentNames { get; set; } = ["Development", "Dev", "Local"];
}
