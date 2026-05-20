using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Odin.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    public const string DefaultAzureKeyVaultConfigurationSectionName = "AzureKeyVault";

    /// <summary>
    /// Adds Azure Key Vault secrets filtered by a prefix to configuration.
    /// </summary>
    /// <param name="configBuilder">The configuration builder.</param>
    /// <param name="azureKeyVaultConfigurationSectionName">Name of the configuration section, 'AzureKeyVault' by default..
    /// The IConfiguration section must have been previously populated to contain entries to satisfy the below:
    ///  - bool Enabled (optional, defaults to true)
    ///  - string VaultName or VaultUri (required)
    ///  - string TenantId (required)
    ///  - string ClientId (required)
    ///  - string Secret (required)
    ///  - string Prefix (optional)
    /// </param>
    /// <param name="options">The optional configuration options for AzureKeyVault load.
    /// If passed, the Manager property is set to PrefixedAzureKeyVaultSecretManager.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddOdinPrefixedAzureKeyVault(this IConfigurationBuilder configBuilder,
        string azureKeyVaultConfigurationSectionName = DefaultAzureKeyVaultConfigurationSectionName,
        AzureKeyVaultConfigurationOptions? options = null)
    {
        IConfigurationRoot tempConfig = configBuilder.Build();
        IConfigurationSection? section = tempConfig.GetSection(azureKeyVaultConfigurationSectionName);
        if (section == null!)
        {
            throw new InvalidOperationException(
                $"{nameof(AddOdinPrefixedAzureKeyVault)}: {azureKeyVaultConfigurationSectionName} configuration section not found.");
        }
        return AddOdinPrefixedAzureKeyVault(configBuilder, section, options);
    }

    /// <summary>
    /// Adds Azure Key Vault secrets filtered by a prefix to configuration.
    /// </summary>
    /// <param name="configBuilder">The configuration builder.</param>
    /// <param name="akvConfigSection">AzureKeyVault configuration section</param>
    /// <param name="options">The optional configuration options for AzureKeyVault load.
    /// If passed, the Manager property is set to PrefixedAzureKeyVaultSecretManager.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddOdinPrefixedAzureKeyVault(this IConfigurationBuilder configBuilder,
        IConfigurationSection akvConfigSection,
        AzureKeyVaultConfigurationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(akvConfigSection);
        string? keyVaultEnabledSetting = akvConfigSection["Enabled"];
        if (!bool.TryParse(keyVaultEnabledSetting, out bool enabled))
        {
            enabled = true; // Default scenario is enabled, unless Enabled is explicitly set to false;
        }
        if (!enabled) return configBuilder; // Abort if disabled.
        
        string? keyVaultName = akvConfigSection["VaultName"]!;
        string? keyVaultUri = akvConfigSection["VaultUri"]!;
        string? tenantId = akvConfigSection["TenantId"]!;
        string? clientId = akvConfigSection["ClientId"]!;
        string? secret = akvConfigSection!["Secret"];
        string? prefix = akvConfigSection!["Prefix"];

        bool nameOrUriExists = !string.IsNullOrWhiteSpace(keyVaultUri) || !string.IsNullOrWhiteSpace(keyVaultName);

        // Silently return if all required config is not present.
        if (!nameOrUriExists || string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(secret))
        {
                throw new InvalidOperationException(
                    $"{nameof(AddOdinPrefixedAzureKeyVault)}: Configuration section requires all of " +
                    $"TenantId, ClientId, Secret and either of VaultName or VaultUri.");
        }

        ClientSecretCredential secretCredentials = new ClientSecretCredential(tenantId, clientId, secret);
        if (!string.IsNullOrWhiteSpace(keyVaultName))
        {
            return configBuilder.AddOdinPrefixedAzureKeyVault(keyVaultName, prefix, secretCredentials, options);
        }

        Uri akvUri = new Uri(keyVaultUri); // Allow an invalid VaultUri to throw an Exception.
        return configBuilder.AddOdinPrefixedAzureKeyVault(akvUri, prefix, secretCredentials, options);
    }

    /// <summary>
    /// Adds Azure Key Vault secrets filtered by a prefix to configuration.
    /// Skips configuration loading gracefully if configuration values are missing.
    /// </summary>
    /// <param name="configBuilder">The configuration builder.</param>
    /// <param name="azureKeyVaultName">The Azure Key Vault name or URI. i.e. for 'https://MyVault.vault.azure.net',
    /// use 'MyVault'</param>
    /// <param name="prefix">Optional prefix to filter secrets by.
    /// Secrets starting with this prefix will be loaded with the prefix stripped.</param>
    /// <param name="credential">The token credential to use for authentication.</param>
    /// <param name="options">The optional configuration options for Azure Key Vault load.
    /// If passed, the Manager property is set to PrefixedAzureKeyVaultSecretManager.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddOdinPrefixedAzureKeyVault(this IConfigurationBuilder configBuilder,
        string azureKeyVaultName,
        string? prefix,
        TokenCredential credential,
        AzureKeyVaultConfigurationOptions? options = null)
    {
        // Guard Clauses
        ArgumentNullException.ThrowIfNull(configBuilder);
        ArgumentNullException.ThrowIfNull(credential);
        ArgumentException.ThrowIfNullOrWhiteSpace(azureKeyVaultName);
        Uri vaultUri = new Uri($"https://{azureKeyVaultName.Trim()}.vault.azure.net/");
        return AddOdinPrefixedAzureKeyVault(configBuilder, vaultUri, prefix, credential, options);
    }

    /// <summary>
    /// Adds Azure Key Vault secrets filtered by a prefix to configuration.
    /// Skips configuration loading gracefully if configuration values are missing.
    /// </summary>
    /// <param name="configBuilder"></param>
    /// <param name="azureKeyVaultUri"></param>
    /// <param name="prefix"></param>
    /// <param name="credential"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IConfigurationBuilder AddOdinPrefixedAzureKeyVault(this IConfigurationBuilder configBuilder,
        Uri azureKeyVaultUri,
        string? prefix,
        TokenCredential credential,
        AzureKeyVaultConfigurationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(configBuilder);
        ArgumentNullException.ThrowIfNull(azureKeyVaultUri);
        ArgumentNullException.ThrowIfNull(credential);

        prefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix.Trim();
        options ??= new AzureKeyVaultConfigurationOptions();
        options.Manager = new PrefixedAzureKeyVaultSecretManager(prefix);
        return configBuilder.AddAzureKeyVault(azureKeyVaultUri, credential, options);
    }
}