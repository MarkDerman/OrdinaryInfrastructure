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

    /// <summary>
    /// Adds the standard host configuration sources used by Soulv applications.
    /// </summary>
    /// <param name="configuration">The host configuration manager to add sources to.</param>
    /// <param name="applicationName">
    /// The logical application name. This is used as the default user secrets id and to derive the default Key Vault prefix.
    /// </param>
    /// <param name="configure">An optional action used to override source names, user secrets, environment variables, or Key Vault behavior.</param>
    /// <returns>The same <see cref="ConfigurationManager"/> so calls can be chained.</returns>
    /// <remarks>
    /// This method centralises the normal host startup pattern: load app settings first, then host-specific sources,
    /// then user secrets, environment variables, and finally Azure Key Vault. Later sources override earlier sources.
    /// </remarks>
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

        configuration.AddJsonFile(GetAppSettingsPath(configuration, options), false, true);

        options.AddExtraConfigurationSources?.Invoke(configuration);

        if (!string.IsNullOrWhiteSpace(options.UserSecretsId))
        {
            configuration.AddUserSecrets(options.UserSecretsId);
        }

        if (options.AddEnvironmentVariables)
        {
            configuration.AddEnvironmentVariables();
        }

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
        TokenCredential? credential = CreateKeyVaultCredential(configuration, section, options);
        if (credential == null)
        {
            return configuration;
        }

        configuration.AddOdinPrefixedAzureKeyVault(keyVaultName, prefix, credential);
        return configuration;
    }

    internal static TokenCredential? CreateKeyVaultCredential(
        IConfiguration configuration,
        IConfigurationSection section,
        KeyVaultConfigurationOptions options)
    {
        string? configuredClientSecret = FirstNonBlank(section[KeyVaultClientSecretKey], section[SecretKey]);
        if (configuredClientSecret != null)
        {
            return CreateClientSecretCredential(
                configuration,
                section,
                options,
                configuredClientSecret,
                useConfiguredClientIdFirst: true);
        }

        string? environmentClientSecret = FirstNonBlank(
            Environment.GetEnvironmentVariable(AzureKeyVaultSecretEnvironmentVariable));
        if (environmentClientSecret != null)
        {
            return CreateClientSecretCredential(
                configuration,
                section,
                options,
                environmentClientSecret,
                useConfiguredClientIdFirst: false);
        }

        string? managedIdentityClientId = FirstNonBlank(section[ManagedIdentityClientIdKey]);
        if (managedIdentityClientId != null)
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
        string clientSecret,
        bool useConfiguredClientIdFirst)
    {
        string? tenantId = FirstNonBlank(
            configuration[options.AzureAdTenantIdKey],
            Environment.GetEnvironmentVariable(AzureKeyVaultTenantIdEnvironmentVariable),
            Environment.GetEnvironmentVariable(AzureTenantIdEnvironmentVariable));

        string? clientId = GetClientIdForClientSecret(
            section,
            useConfiguredClientIdFirst);

        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
        {
            throw new ApplicationException(MissingClientSecretCredentialConfigurationMessage);
        }

        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

    internal static string? GetClientIdForClientSecret(
        IConfigurationSection section,
        bool useConfiguredClientIdFirst)
    {
        string? configuredClientId = FirstNonBlank(section[KeyVaultClientIdKey], section[ClientIdKey]);
        string? environmentClientId = FirstNonBlank(
            Environment.GetEnvironmentVariable(AzureKeyVaultClientIdEnvironmentVariable));

        return useConfiguredClientIdFirst
            ? FirstNonBlank(configuredClientId, environmentClientId)
            : FirstNonBlank(environmentClientId, configuredClientId);
    }

    private static bool IsLocalEnvironment(
        IConfiguration configuration,
        KeyVaultConfigurationOptions options)
    {
        string runtimeEnvironment = FirstNonBlank(
            configuration[options.EnvironmentKey],
            options.DefaultEnvironmentName)!.Trim();

        return options.LocalEnvironmentNames.Any(
            localName => runtimeEnvironment.Equals(localName?.Trim(), StringComparison.OrdinalIgnoreCase));
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
        string prefixApplicationName = FirstNonBlank(options.PrefixApplicationName)?.Trim()
            ?? GetDefaultPrefixApplicationName(applicationName);
        string environment = FirstNonBlank(configuration[options.EnvironmentKey], options.DefaultEnvironmentName)!.Trim();
        return $"{prefixApplicationName}-{environment}-";
    }

    internal static string GetKeyVaultPrefix(
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
                return value.Trim();
            }
        }

        return null;
    }
}
