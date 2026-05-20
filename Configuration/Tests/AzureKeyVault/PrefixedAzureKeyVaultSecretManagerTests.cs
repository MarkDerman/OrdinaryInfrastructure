using Azure.Security.KeyVault.Secrets;
using Odin.Configuration;

namespace Tests.Odin.Configuration.AzureKeyVault;

public class PrefixedAzureKeyVaultSecretManagerTests
{
    [Theory]
    [InlineData("MyApp-Production-", "MyApp-Production-ConnectionStrings-Default", true)]
    [InlineData("MyApp-Production-", "OtherApp-Production-ConnectionStrings-Default", false)]
    [InlineData("", "OtherApp-Production-ConnectionStrings-Default", true)]
    [InlineData(null, "OtherApp-Production-ConnectionStrings-Default", true)]
    [InlineData("   ", "OtherApp-Production-ConnectionStrings-Default", true)]
    public void Load_returns_true_only_for_secrets_that_start_with_the_prefix(
        string? prefix,
        string secretName,
        bool expected)
    {
        PrefixedAzureKeyVaultSecretManager sut = new(prefix);
        SecretProperties secretProperties = new(secretName);

        bool result = sut.Load(secretProperties);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MyApp-Production-", "MyApp-Production-ConnectionStrings-Default", "ConnectionStrings:Default")]
    [InlineData("MyApp-Production-", "MyApp-Production-Logging-Minimum-Level", "Logging:Minimum:Level")]
    [InlineData("", "ConnectionStrings-Default", "ConnectionStrings:Default")]
    [InlineData(null, "ConnectionStrings-Default", "ConnectionStrings:Default")]
    [InlineData("   ", "ConnectionStrings-Default", "ConnectionStrings:Default")]
    public void GetKey_removes_the_prefix_and_maps_hyphens_to_configuration_separators(
        string? prefix,
        string secretName,
        string expected)
    {
        PrefixedAzureKeyVaultSecretManager sut = new(prefix);
        KeyVaultSecret secret = new(secretName, "secret-value");

        string result = sut.GetKey(secret);

        Assert.Equal(expected, result);
    }
}
