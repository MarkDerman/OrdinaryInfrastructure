using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;
using Odin.Configuration;

namespace Tests.Odin.Configuration.AzureKeyVault;

public class ConfigurationBuilderExtensionsTests
{
    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_disabled_section_returns_the_same_builder_without_adding_a_source()
    {
        IConfigurationRoot config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:Enabled"] = "false"
        });
        IConfigurationBuilder builder = new ConfigurationBuilder();

        IConfigurationBuilder result = builder.AddOdinPrefixedAzureKeyVault(config.GetSection("AzureKeyVault"));

        Assert.That(result, Is.SameAs(builder));
        Assert.That(builder.Sources, Is.Empty);
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_missing_required_section_values_throws()
    {
        IConfigurationRoot config = CreateConfiguration(new Dictionary<string, string?>
        {
            ["AzureKeyVault:Enabled"] = "true",
            ["AzureKeyVault:VaultName"] = "odin-test-vault"
        });
        IConfigurationBuilder builder = new ConfigurationBuilder();

        InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => builder.AddOdinPrefixedAzureKeyVault(config.GetSection("AzureKeyVault")));

        Assert.That(result.Message, Does.Contain("TenantId, ClientId, Secret"));
        Assert.That(builder.Sources, Is.Empty);
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_section_name_reads_configuration_from_the_builder()
    {
        AzureKeyVaultConfigurationOptions options = new();
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddInMemoryCollection(CreateRequiredConfiguration(prefix: "  MyApp-Production-  "));

        IConfigurationBuilder result = builder.AddOdinPrefixedAzureKeyVault(options: options);

        Assert.That(result, Is.SameAs(builder));
        builder.Sources.OfType<AzureKeyVaultConfigurationSource>().Single();
        Assert.That(options.Manager, Is.TypeOf<PrefixedAzureKeyVaultSecretManager>());
        PrefixedAzureKeyVaultSecretManager manager = (PrefixedAzureKeyVaultSecretManager)options.Manager;
        Assert.That(manager.Load(new("MyApp-Production-ConnectionStrings-Default")), Is.True);
        Assert.That(manager.Load(new("OtherApp-Production-ConnectionStrings-Default")), Is.False);
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_section_uses_vault_uri_when_vault_name_is_not_configured()
    {
        AzureKeyVaultConfigurationOptions options = new();
        IConfigurationRoot config = CreateConfiguration(CreateRequiredConfiguration(
            vaultName: null,
            vaultUri: "https://odin-test-vault.vault.azure.net/",
            prefix: "MyApp-Production-"));
        IConfigurationBuilder builder = new ConfigurationBuilder();

        IConfigurationBuilder result = builder.AddOdinPrefixedAzureKeyVault(config.GetSection("AzureKeyVault"), options);

        Assert.That(result, Is.SameAs(builder));
        builder.Sources.OfType<AzureKeyVaultConfigurationSource>().Single();
        Assert.That(options.Manager, Is.TypeOf<PrefixedAzureKeyVaultSecretManager>());
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_uri_adds_key_vault_source_and_replaces_the_options_manager()
    {
        AzureKeyVaultConfigurationOptions options = new()
        {
            ReloadInterval = TimeSpan.FromMinutes(5)
        };
        IConfigurationBuilder builder = new ConfigurationBuilder();

        IConfigurationBuilder result = builder.AddOdinPrefixedAzureKeyVault(
            new Uri("https://odin-test-vault.vault.azure.net/"),
            "  MyApp-Production-  ",
            new TestTokenCredential(),
            options);

        Assert.That(result, Is.SameAs(builder));
        builder.Sources.OfType<AzureKeyVaultConfigurationSource>().Single();
        Assert.That(options.ReloadInterval, Is.EqualTo(TimeSpan.FromMinutes(5)));
        Assert.That(options.Manager, Is.TypeOf<PrefixedAzureKeyVaultSecretManager>());
        PrefixedAzureKeyVaultSecretManager manager = (PrefixedAzureKeyVaultSecretManager)options.Manager;
        Assert.That(manager.Load(new("MyApp-Production-ConnectionStrings-Default")), Is.True);
        Assert.That(manager.Load(new("OtherApp-Production-ConnectionStrings-Default")), Is.False);
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_vault_name_adds_key_vault_source()
    {
        AzureKeyVaultConfigurationOptions options = new();
        IConfigurationBuilder builder = new ConfigurationBuilder();

        IConfigurationBuilder result = builder.AddOdinPrefixedAzureKeyVault(
            " odin-test-vault ",
            "MyApp-Production-",
            new TestTokenCredential(),
            options);

        Assert.That(result, Is.SameAs(builder));
        builder.Sources.OfType<AzureKeyVaultConfigurationSource>().Single();
        Assert.That(options.Manager, Is.TypeOf<PrefixedAzureKeyVaultSecretManager>());
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_section_throws_when_section_is_null()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.AddOdinPrefixedAzureKeyVault(akvConfigSection: null!));
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_vault_name_throws_for_invalid_arguments()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();
        TestTokenCredential credential = new();

        Assert.Throws<ArgumentNullException>(() => Microsoft.Extensions.Configuration.ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault(
                null!,
                "odin-test-vault",
                "MyApp-Production-",
                credential));
        Assert.Throws<ArgumentException>(() => builder.AddOdinPrefixedAzureKeyVault(
                "   ",
                "MyApp-Production-",
                credential));
        Assert.Throws<ArgumentNullException>(() => builder.AddOdinPrefixedAzureKeyVault(
                "odin-test-vault",
                "MyApp-Production-",
                credential: null!));
    }

    [Test]
    public void AddOdinPrefixedAzureKeyVault_with_uri_throws_for_invalid_arguments()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();
        Uri vaultUri = new("https://odin-test-vault.vault.azure.net/");
        TestTokenCredential credential = new();

        Assert.Throws<ArgumentNullException>(() => Microsoft.Extensions.Configuration.ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault(
                null!,
                vaultUri,
                "MyApp-Production-",
                credential));
        Assert.Throws<ArgumentNullException>(() => builder.AddOdinPrefixedAzureKeyVault(
                azureKeyVaultUri: null!,
                prefix: "MyApp-Production-",
                credential: credential));
        Assert.Throws<ArgumentNullException>(() => builder.AddOdinPrefixedAzureKeyVault(
                vaultUri,
                "MyApp-Production-",
                credential: null!));
    }

    private static IConfigurationRoot CreateConfiguration(IEnumerable<KeyValuePair<string, string?>> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static Dictionary<string, string?> CreateRequiredConfiguration(
        string? vaultName = "odin-test-vault",
        string? vaultUri = null,
        string? prefix = "MyApp-Production-")
    {
        Dictionary<string, string?> values = new()
        {
            ["AzureKeyVault:TenantId"] = "11111111-1111-1111-1111-111111111111",
            ["AzureKeyVault:ClientId"] = "22222222-2222-2222-2222-222222222222",
            ["AzureKeyVault:Secret"] = "client-secret-value",
            ["AzureKeyVault:Prefix"] = prefix
        };

        if (vaultName is not null)
        {
            values["AzureKeyVault:VaultName"] = vaultName;
        }

        if (vaultUri is not null)
        {
            values["AzureKeyVault:VaultUri"] = vaultUri;
        }

        return values;
    }

    private sealed class TestTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(GetToken(requestContext, cancellationToken));
        }
    }
}
