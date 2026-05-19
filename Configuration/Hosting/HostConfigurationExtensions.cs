using Azure.Core;
using Azure.Identity;
using Odin.Configuration.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class HostConfigurationExtensions
{
    private const string KeyVaultNameKey = "Name";
    private const string KeyVaultPrefixKey = "Prefix";
    private const string SkipKeyVaultConfigInjectionKey = "SkipKeyVaultConfigInjection";
    private const string KeyVaultClientIdKey = "KeyVaultClientId";
    private const string ClientIdKey = "ClientId";
    private const string KeyVaultClientSecretKey = "KeyVaultClientSecret";
    private const string SecretKey = "Secret";
    private const string ManagedIdentityClientIdKey = "ManagedIdentityClientId";
    private const string AzureKeyVaultTenantIdEnvironmentVariable = "AZUREKEYVAULT_TENANT_ID";
    private const string AzureTenantIdEnvironmentVariable = "AZURE_TENANT_ID";
    private const string AzureKeyVaultClientIdEnvironmentVariable = "AZUREKEYVAULT_CLIENT_ID";
    private const string AzureKeyVaultSecretEnvironmentVariable = "AZUREKEYVAULT_SECRET";

    public const string MissingClientSecretCredentialConfigurationMessage =
        "Azure Key Vault client secret configured, but tenant id or client id is missing.";

    public const string MissingKeyVaultCredentialConfigurationMessage =
        "Azure Key Vault is configured, but no Key Vault credential is configured. " +
        "Configure client secret credentials, configure AzureKeyVault:" + ManagedIdentityClientIdKey + ", " +
        "set AzureKeyVault:" + SkipKeyVaultConfigInjectionKey + "=true, or remove AzureKeyVault:" + KeyVaultNameKey + ".";

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

        AddPrimaryJsonConfiguration(configuration, options);

        // Host-specific sources load before secrets/env vars; they refine app defaults, not deployment secrets.
        options.ConfigureAdditionalSources?.Invoke(configuration);

        AddUserSecretsIfConfigured(configuration, options);
        AddEnvironmentVariablesIfEnabled(configuration, options);

        // Key Vault loads last so real secret values replace placeholders such as KEYVAULT.
        AddKeyVaultConfiguration(configuration, applicationName, options.KeyVault);

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

        return AddKeyVaultConfiguration(configuration, applicationName, options);
    }

    private static ConfigurationManager AddKeyVaultConfiguration(
        ConfigurationManager configuration,
        string applicationName,
        KeyVaultConfigurationOptions options)
    {
        IConfigurationSection section = configuration.GetSection(options.SectionName);
        if (section.GetValue<bool?>(SkipKeyVaultConfigInjectionKey) ?? false)
        {
            return configuration;
        }

        string? keyVaultName = section[KeyVaultNameKey];
        if (string.IsNullOrWhiteSpace(keyVaultName))
        {
            return configuration;
        }

        string prefix = GetKeyVaultPrefix(section, configuration, applicationName, options);
        TokenCredential? credential = CreateKeyVaultCredential(configuration, options);
        if (credential == null)
        {
            return configuration;
        }

        configuration.AddOdinPrefixedAzureKeyVault(keyVaultName, prefix, credential);
        return configuration;
    }

    private static TokenCredential? CreateKeyVaultCredential(
        IConfiguration configuration,
        KeyVaultConfigurationOptions options)
    {
        IConfigurationSection section = configuration.GetSection(options.SectionName);
        string? configuredClientSecret = FirstNonBlank(section[KeyVaultClientSecretKey], section[SecretKey]);
        string? environmentClientSecret = FirstNonBlank(
            Environment.GetEnvironmentVariable(AzureKeyVaultSecretEnvironmentVariable));

        string? clientSecret = FirstNonBlank(configuredClientSecret, environmentClientSecret);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            return CreateClientSecretCredential(
                configuration,
                section,
                options,
                configuredClientSecret,
                clientSecret);
        }

        string? managedIdentityClientId = FirstNonBlank(section[ManagedIdentityClientIdKey]);
        if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            return new ManagedIdentityCredential(
                ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));
        }

        if (IsLocalEnvironment(configuration, options))
        {
            // Local development can run without Key Vault so tests/dev machines do not require Azure identity.
            return null;
        }

        throw new ApplicationException(MissingKeyVaultCredentialConfigurationMessage);
    }

    private static ClientSecretCredential CreateClientSecretCredential(
        IConfiguration configuration,
        IConfigurationSection section,
        KeyVaultConfigurationOptions options,
        string? configuredClientSecret,
        string clientSecret)
    {
        string? tenantId = FirstNonBlank(
            configuration[options.AzureAdTenantIdKey],
            Environment.GetEnvironmentVariable(AzureKeyVaultTenantIdEnvironmentVariable),
            Environment.GetEnvironmentVariable(AzureTenantIdEnvironmentVariable));

        string? clientId = GetClientIdForClientSecret(
            section,
            preferConfiguredClientId: !string.IsNullOrWhiteSpace(configuredClientSecret));

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            throw new ApplicationException(MissingClientSecretCredentialConfigurationMessage);
        }

        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

    private static string? GetClientIdForClientSecret(
        IConfigurationSection section,
        bool preferConfiguredClientId)
    {
        string? configuredClientId = FirstNonBlank(section[KeyVaultClientIdKey], section[ClientIdKey]);
        string? environmentClientId = FirstNonBlank(
            Environment.GetEnvironmentVariable(AzureKeyVaultClientIdEnvironmentVariable));

        return preferConfiguredClientId
            ? FirstNonBlank(configuredClientId, environmentClientId)
            : FirstNonBlank(environmentClientId, configuredClientId);
    }

    private static bool IsLocalEnvironment(
        IConfiguration configuration,
        KeyVaultConfigurationOptions options)
    {
        string runtimeEnvironment = FirstNonBlank(
            configuration[options.EnvironmentKey],
            options.DefaultEnvironmentName)!;

        return options.LocalEnvironmentNames.Any(
            localName => runtimeEnvironment.Equals(localName, StringComparison.OrdinalIgnoreCase));
    }

    private static void AddPrimaryJsonConfiguration(
        ConfigurationManager configuration,
        HostConfigurationOptions options)
    {
        // Base JSON loads first so later sources can override placeholders and environment defaults.
        configuration.AddJsonFile(GetAppSettingsPath(configuration, options), false, true);
    }

    private static void AddUserSecretsIfConfigured(
        ConfigurationManager configuration,
        HostConfigurationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.UserSecretsId))
        {
            return;
        }

        // User secrets default to applicationName, but hosts can use a separate local secret bucket.
        configuration.AddUserSecrets(options.UserSecretsId);
    }

    private static void AddEnvironmentVariablesIfEnabled(
        ConfigurationManager configuration,
        HostConfigurationOptions options)
    {
        if (!options.AddEnvironmentVariables)
        {
            return;
        }

        // Environment variables override JSON and user secrets for deployment-time configuration.
        configuration.AddEnvironmentVariables();
    }

    private static string GetAppSettingsPath(
        IConfigurationBuilder configuration,
        HostConfigurationOptions options)
    {
        string configFolderAppSettings = Path.Combine(options.ConfigFolderName, options.AppSettingsFileName);
        return options.PreferConfigFolder && configuration.GetFileProvider().GetFileInfo(configFolderAppSettings).Exists
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

    private static string GetKeyVaultPrefix(
        IConfigurationSection section,
        IConfiguration configuration,
        string applicationName,
        KeyVaultConfigurationOptions options)
    {
        string? configuredPrefix = FirstNonBlank(section[KeyVaultPrefixKey])?.Trim();
        return configuredPrefix ?? GetDefaultKeyVaultPrefix(configuration, applicationName, options);
    }

    private static string GetDefaultPrefixApplicationName(string applicationName)
    {
        string[] parts = applicationName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 0 ? applicationName : parts[^1];
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
