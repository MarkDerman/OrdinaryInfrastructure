using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Odin.Configuration.Hosting;
using System.Reflection;

namespace Tests.Odin.Configuration.Hosting;

[CollectionDefinition(nameof(HostConfigurationExtensionsTests), DisableParallelization = true)]
public sealed class HostConfigurationExtensionsTestsCollection;

[Collection(nameof(HostConfigurationExtensionsTests))]
public sealed class HostConfigurationExtensionsTests : IDisposable
{
    private const string AzureKeyVaultClientId = "AZUREKEYVAULT_CLIENT_ID";
    private const string AzureKeyVaultSecret = "AZUREKEYVAULT_SECRET";
    private const string AzureKeyVaultTenantId = "AZUREKEYVAULT_TENANT_ID";
    private const string AzureTenantId = "AZURE_TENANT_ID";
    private const string TestSettingName = "ODIN_HOST_CONFIG_TEST_VALUE";

    private readonly Dictionary<string, string?> _originalEnvironmentValues = new();
    private readonly string _originalDirectory;

    public HostConfigurationExtensionsTests()
    {
        _originalDirectory = Environment.CurrentDirectory;
        CaptureAndClear(AzureKeyVaultClientId);
        CaptureAndClear(AzureKeyVaultSecret);
        CaptureAndClear(AzureKeyVaultTenantId);
        CaptureAndClear(AzureTenantId);
        CaptureAndClear(TestSettingName);
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _originalDirectory;
        foreach ((string name, string? value) in _originalEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    [Fact]
    public void AddHostConfiguration_prefers_config_folder_appsettings_when_present()
    {
        using TemporaryConfigurationDirectory directory = TemporaryConfigurationDirectory.Create();
        directory.WriteJson("appSettings.json", TestSettingName, "root");
        directory.WriteJson(Path.Combine("config", "appSettings.json"), TestSettingName, "config");

        ConfigurationManager manager = new();
        manager.SetBasePath(directory.DirectoryPath);
        manager.AddHostConfiguration("Test.App", options =>
        {
            options.UserSecretsId = null;
            options.AddEnvironmentVariables = false;
        });

        Assert.Equal("config", manager[TestSettingName]);
    }

    [Fact]
    public void AddHostConfiguration_uses_root_appsettings_when_config_folder_missing()
    {
        using TemporaryConfigurationDirectory directory = TemporaryConfigurationDirectory.Create();
        directory.WriteJson("appSettings.json", TestSettingName, "root");

        ConfigurationManager manager = new();
        manager.SetBasePath(directory.DirectoryPath);
        manager.AddHostConfiguration("Test.App", options =>
        {
            options.UserSecretsId = null;
            options.AddEnvironmentVariables = false;
        });

        Assert.Equal("root", manager[TestSettingName]);
    }

    [Fact]
    public void AddHostConfiguration_applies_additional_sources_after_appsettings()
    {
        using TemporaryConfigurationDirectory directory = TemporaryConfigurationDirectory.Create();
        directory.WriteJson("appSettings.json", TestSettingName, "root");

        ConfigurationManager manager = new();
        manager.SetBasePath(directory.DirectoryPath);
        manager.AddHostConfiguration("Test.App", options =>
        {
            options.UserSecretsId = null;
            options.AddEnvironmentVariables = false;
            options.AddExtraConfigurationSources = configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [TestSettingName] = "additional"
                });
        });

        Assert.Equal("additional", manager[TestSettingName]);
    }

    [Fact]
    public void AddHostConfiguration_applies_environment_variables_after_additional_sources()
    {
        using TemporaryConfigurationDirectory directory = TemporaryConfigurationDirectory.Create();
        directory.WriteJson("appSettings.json", TestSettingName, "root");
        Environment.SetEnvironmentVariable(TestSettingName, "environment");

        ConfigurationManager manager = new();
        manager.SetBasePath(directory.DirectoryPath);
        manager.AddHostConfiguration("Test.App", options =>
        {
            options.UserSecretsId = null;
            options.AddExtraConfigurationSources = configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [TestSettingName] = "additional"
                });
        });

        Assert.Equal("environment", manager[TestSettingName]);
    }

    [Fact]
    public void AddHostConfiguration_prefers_config_folder_from_configuration_base_path()
    {
        using TemporaryConfigurationDirectory currentDirectory = TemporaryConfigurationDirectory.Create();
        currentDirectory.WriteJson(Path.Combine("config", "appSettings.json"), TestSettingName, "current");

        using TemporaryConfigurationDirectory basePathDirectory = TemporaryConfigurationDirectory.Create(useAsCurrentDirectory: false);
        basePathDirectory.WriteJson("appSettings.json", TestSettingName, "base-root");
        basePathDirectory.WriteJson(Path.Combine("config", "appSettings.json"), TestSettingName, "base-config");

        ConfigurationManager manager = new();
        manager.SetBasePath(basePathDirectory.DirectoryPath);
        manager.AddHostConfiguration("Test.App", options =>
        {
            options.UserSecretsId = null;
            options.AddEnvironmentVariables = false;
        });

        Assert.Equal("base-config", manager[TestSettingName]);
    }

    [Fact]
    public void Missing_key_vault_name_skips_key_vault_configuration()
    {
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:KeyVaultClientSecret"] = "configured-secret"
        });

        Exception? exception = Record.Exception(() => manager.AddKeyVaultConfiguration("Test.App"));

        Assert.Null(exception);
    }

    [Fact]
    public void Skip_flag_disables_key_vault_configuration()
    {
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:Name"] = "test-vault",
            ["AzureKeyVault:SkipKeyVaultConfigInjection"] = "true",
            ["AzureKeyVault:KeyVaultClientSecret"] = "configured-secret"
        });

        Exception? exception = Record.Exception(() => manager.AddKeyVaultConfiguration("Test.App"));

        Assert.Null(exception);
    }

    [Fact]
    public void Client_secret_without_tenant_or_client_id_throws_clear_configuration_error()
    {
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:Name"] = "test-vault",
            ["AzureKeyVault:KeyVaultClientSecret"] = "configured-secret"
        });

        ApplicationException exception = Assert.Throws<ApplicationException>(
            () => manager.AddKeyVaultConfiguration("Test.App"));

        Assert.Equal(
            HostConfigurationExtensions.MissingClientSecretCredentialConfigurationMessage,
            exception.Message);
    }

    [Fact]
    public void Whitespace_configured_client_secret_falls_back_to_environment_client_secret()
    {
        Environment.SetEnvironmentVariable(AzureKeyVaultClientId, "environment-client-id");
        Environment.SetEnvironmentVariable(AzureKeyVaultSecret, "environment-secret");
        Environment.SetEnvironmentVariable(AzureKeyVaultTenantId, "tenant-id");

        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:KeyVaultClientSecret"] = " "
        });

        object? credential = CreateKeyVaultCredential(manager);

        Assert.IsType<ClientSecretCredential>(credential);
    }

    [Fact]
    public void Configured_client_secret_prefers_configured_client_id()
    {
        Environment.SetEnvironmentVariable(AzureKeyVaultClientId, "environment-client-id");
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:KeyVaultClientId"] = "configured-client-id"
        });

        string? clientId = GetClientIdForClientSecret(manager, preferConfiguredClientId: true);

        Assert.Equal("configured-client-id", clientId);
    }

    [Fact]
    public void Environment_client_secret_prefers_environment_client_id()
    {
        Environment.SetEnvironmentVariable(AzureKeyVaultClientId, "environment-client-id");
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:KeyVaultClientId"] = "configured-client-id"
        });

        string? clientId = GetClientIdForClientSecret(manager, preferConfiguredClientId: false);

        Assert.Equal("environment-client-id", clientId);
    }

    [Fact]
    public void Configured_key_vault_prefix_overrides_default_prefix()
    {
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Environment"] = "Production",
            ["AzureKeyVault:Prefix"] = " Pinned- "
        });

        string prefix = GetKeyVaultPrefix(manager, "Test.App");

        Assert.Equal("Pinned-", prefix);
    }

    [Fact]
    public void Development_without_key_vault_credential_skips_key_vault_configuration()
    {
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Environment"] = "Development",
            ["AzureKeyVault:Name"] = "test-vault"
        });

        Exception? exception = Record.Exception(() => manager.AddKeyVaultConfiguration("Test.App"));

        Assert.Null(exception);
    }

    [Fact]
    public void Non_development_key_vault_without_key_vault_credential_throws_clear_configuration_error()
    {
        ConfigurationManager manager = CreateConfiguration(new Dictionary<string, string?>
        {
            ["Environment"] = "Production",
            ["AzureKeyVault:Name"] = "test-vault"
        });

        ApplicationException exception = Assert.Throws<ApplicationException>(
            () => manager.AddKeyVaultConfiguration("Test.App"));

        Assert.Equal(
            HostConfigurationExtensions.MissingKeyVaultCredentialConfigurationMessage,
            exception.Message);
    }

    private static ConfigurationManager CreateConfiguration(Dictionary<string, string?> values)
    {
        ConfigurationManager manager = new();
        manager.AddInMemoryCollection(values);
        return manager;
    }

    private static object? CreateKeyVaultCredential(ConfigurationManager manager)
    {
        MethodInfo method = typeof(HostConfigurationExtensions).GetMethod(
            "CreateKeyVaultCredential",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        return method.Invoke(null, [manager, new KeyVaultConfigurationOptions()]);
    }

    private static string? GetClientIdForClientSecret(
        ConfigurationManager manager,
        bool preferConfiguredClientId)
    {
        MethodInfo method = typeof(HostConfigurationExtensions).GetMethod(
            "GetClientIdForClientSecret",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        return (string?)method.Invoke(
            null,
            [manager.GetSection("AzureKeyVault"), preferConfiguredClientId]);
    }

    private static string GetKeyVaultPrefix(ConfigurationManager manager, string applicationName)
    {
        MethodInfo method = typeof(HostConfigurationExtensions).GetMethod(
            "GetKeyVaultPrefix",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        return (string)method.Invoke(
            null,
            [manager.GetSection("AzureKeyVault"), manager, applicationName, new KeyVaultConfigurationOptions()])!;
    }

    private void CaptureAndClear(string name)
    {
        _originalEnvironmentValues[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, null);
    }

    private sealed class TemporaryConfigurationDirectory : IDisposable
    {
        private readonly string _path;

        public string DirectoryPath => _path;

        private TemporaryConfigurationDirectory(string path)
        {
            _path = path;
        }

        public static TemporaryConfigurationDirectory Create(bool useAsCurrentDirectory = true)
        {
            string path = Path.Combine(Path.GetTempPath(), $"host-config-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            if (useAsCurrentDirectory)
            {
                Environment.CurrentDirectory = path;
            }

            return new TemporaryConfigurationDirectory(path);
        }

        public void WriteJson(string relativePath, string key, string value)
        {
            string path = Path.Combine(_path, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, $$"""
                                      {
                                        "{{key}}": "{{value}}"
                                      }
                                      """);
        }

        public void Dispose()
        {
            Directory.Delete(_path, true);
        }
    }
}
