using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Odin.Configuration;
using Odin.Configuration.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class HostConfigurationExtensions
{
    public const string MissingClientSecretCredentialConfigurationMessage =
        "Azure Key Vault client secret configured, but tenant id or client id is missing.";

    public const string MissingKeyVaultCredentialConfigurationMessage =
        "Azure Key Vault is configured, but no Key Vault credential is configured. " +
        "Configure client secret credentials, configure AzureKeyVault:ManagedIdentityClientId, " +
        "set AzureKeyVault:SkipKeyVaultConfigInjection=true, or remove AzureKeyVault:Name.";

    public static ConfigurationManager AddHostConfiguration(
        this ConfigurationManager configuration,
        string applicationName,
        Action<HostConfigurationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        HostConfigurationOptions options = new();
        options.UserSecretsId = applicationName;
        configure?.Invoke(options);

        // Base JSON loads first so later sources can override placeholders and environment defaults.
        configuration.AddJsonFile(GetAppSettingsPath(options), false, true);

        // Host-specific sources load before secrets/env vars; they refine app defaults, not deployment secrets.
        options.AdditionalConfigurationSources?.Invoke(configuration);

        if (!string.IsNullOrWhiteSpace(options.UserSecretsId))
        {
            // User secrets default to applicationName, but hosts can use a separate local secret bucket.
            configuration.AddUserSecrets(options.UserSecretsId);
        }

        if (options.AddEnvironmentVariables)
        {
            // Environment variables override JSON and user secrets for deployment-time configuration.
            configuration.AddEnvironmentVariables();
        }

        // Key Vault loads last so real secret values replace placeholders such as KEYVAULT.
        configuration.AddKeyVaultConfiguration(applicationName, keyVaultOptions =>
        {
            CopyKeyVaultOptions(options.KeyVault, keyVaultOptions);
        });

        return configuration;
    }

    public static ConfigurationManager AddKeyVaultConfiguration(
        this ConfigurationManager configuration,
        string applicationName,
        Action<KeyVaultConfigurationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        KeyVaultConfigurationOptions options = new();
        configure?.Invoke(options);

        IConfigurationSection section = configuration.GetSection(options.SectionName);
        if (section.GetValue<bool?>("SkipKeyVaultConfigInjection") ?? false)
        {
            return configuration;
        }

        string? keyVaultName = section["Name"];
        if (string.IsNullOrWhiteSpace(keyVaultName))
        {
            return configuration;
        }

        string? configuredPrefix = section["Prefix"]?.Trim();
        string prefix = string.IsNullOrWhiteSpace(configuredPrefix)
            // Derive a stable app prefix unless configuration explicitly pins a Key Vault prefix.
            ? GetDefaultKeyVaultPrefix(configuration, applicationName, options)
            : configuredPrefix;

        TokenCredential? credential = CreateKeyVaultCredential(configuration, options);
        if (credential == null)
        {
            return configuration;
        }

        AddPrefixedAzureKeyVault(configuration, keyVaultName, prefix, credential);
        return configuration;
    }

    private static void AddPrefixedAzureKeyVault(
        IConfigurationBuilder configuration,
        string keyVaultName,
        string prefix,
        TokenCredential credential)
    {
        keyVaultName = keyVaultName.Trim().TrimEnd('/');
        Uri vaultUri = keyVaultName.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? new Uri(keyVaultName)
            : new Uri($"https://{keyVaultName}.vault.azure.net/");

        AzureKeyVaultConfigurationOptions azureOptions = new()
        {
            Manager = new PrefixedAzureKeyVaultSecretManager(prefix)
        };

        configuration.AddAzureKeyVault(vaultUri, credential, azureOptions);
    }

    private static TokenCredential? CreateKeyVaultCredential(
        IConfiguration configuration,
        KeyVaultConfigurationOptions options)
    {
        IConfigurationSection section = configuration.GetSection(options.SectionName);
        string? tenantId = configuration[options.AzureAdTenantIdKey]
                           ?? Environment.GetEnvironmentVariable("AZUREKEYVAULT_TENANT_ID")
                           ?? Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
        string? configuredClientId = section["KeyVaultClientId"]
                                     ?? section["ClientId"];
        string? environmentClientId = Environment.GetEnvironmentVariable("AZUREKEYVAULT_CLIENT_ID");
        string? configuredClientSecret = section["KeyVaultClientSecret"]
                                         ?? section["Secret"];
        string? environmentClientSecret = Environment.GetEnvironmentVariable("AZUREKEYVAULT_SECRET");
        string? clientSecret = configuredClientSecret ?? environmentClientSecret;

        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            // Configured secrets prefer configured client id; environment secrets prefer environment client id.
            string? clientId = !string.IsNullOrWhiteSpace(configuredClientSecret)
                ? configuredClientId ?? environmentClientId
                : environmentClientId ?? configuredClientId;

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
            {
                throw new ApplicationException(MissingClientSecretCredentialConfigurationMessage);
            }

            return new ClientSecretCredential(tenantId, clientId, clientSecret);
        }

        string? managedIdentityClientId = section["ManagedIdentityClientId"];
        if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            return new ManagedIdentityCredential(
                ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));
        }

        string environment = configuration[options.EnvironmentKey] ?? options.DefaultEnvironmentName;
        if (options.LocalEnvironmentNames.Any(
                localName => environment.Equals(localName, StringComparison.OrdinalIgnoreCase)))
        {
            // Local development can run without Key Vault so tests/dev machines do not require Azure identity.
            return null;
        }

        throw new ApplicationException(MissingKeyVaultCredentialConfigurationMessage);
    }

    private static string GetAppSettingsPath(HostConfigurationOptions options)
    {
        string configFolderAppSettings = Path.Combine(options.ConfigFolderName, options.AppSettingsFileName);
        return options.PreferConfigFolder && File.Exists(configFolderAppSettings)
            ? configFolderAppSettings
            : options.AppSettingsFileName;
    }

    private static string GetDefaultKeyVaultPrefix(
        IConfiguration configuration,
        string applicationName,
        KeyVaultConfigurationOptions options)
    {
        string prefixApplicationName = string.IsNullOrWhiteSpace(options.PrefixApplicationName)
            ? GetDefaultPrefixApplicationName(applicationName)
            : options.PrefixApplicationName;
        string environment = configuration[options.EnvironmentKey] ?? options.DefaultEnvironmentName;
        return $"{prefixApplicationName}-{environment}-";
    }

    private static string GetDefaultPrefixApplicationName(string applicationName)
    {
        string[] parts = applicationName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 0 ? applicationName : parts[^1];
    }

    private static void CopyKeyVaultOptions(
        KeyVaultConfigurationOptions source,
        KeyVaultConfigurationOptions destination)
    {
        destination.SectionName = source.SectionName;
        destination.AzureAdTenantIdKey = source.AzureAdTenantIdKey;
        destination.DefaultEnvironmentName = source.DefaultEnvironmentName;
        destination.EnvironmentKey = source.EnvironmentKey;
        destination.PrefixApplicationName = source.PrefixApplicationName;
        destination.LocalEnvironmentNames = source.LocalEnvironmentNames;
    }
}
