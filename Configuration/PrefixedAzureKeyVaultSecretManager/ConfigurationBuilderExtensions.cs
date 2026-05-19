using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Odin.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Key Vault secrets filtered by a prefix to configuration.
    /// </summary>
    /// <param name="configBuilder">The configuration builder.</param>
    /// <param name="vaultNameOrUri">The Azure Key Vault name or URI. Use either "https://MyVault.vault.azure.net/" or "MyVault".</param>
    /// <param name="prefix">The prefix to filter secrets by. Loaded secrets have this prefix stripped from their configuration key.</param>
    /// <param name="credential">The token credential to use for authentication.</param>
    /// <param name="options">The optional configuration options for Azure Key Vault.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddOdinPrefixedAzureKeyVault(this IConfigurationBuilder configBuilder,
        string vaultNameOrUri,
        string? prefix,
        TokenCredential credential,
        AzureKeyVaultConfigurationOptions? options = null)
    {
        // Guard Clauses
        ArgumentNullException.ThrowIfNull(configBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(vaultNameOrUri);
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(credential);

        prefix = prefix?.Trim() ?? string.Empty;

        options ??= new AzureKeyVaultConfigurationOptions();
        options.Manager = new PrefixedAzureKeyVaultSecretManager(prefix);

        return configBuilder.AddAzureKeyVault(CreateVaultUri(vaultNameOrUri), credential, options);
    }

    private static Uri CreateVaultUri(string vaultNameOrUri)
    {
        string trimmedVaultNameOrUri = vaultNameOrUri.Trim().TrimEnd('/');
        return trimmedVaultNameOrUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? new Uri(trimmedVaultNameOrUri)
            : new Uri($"https://{trimmedVaultNameOrUri}.vault.azure.net/");
    }
}
