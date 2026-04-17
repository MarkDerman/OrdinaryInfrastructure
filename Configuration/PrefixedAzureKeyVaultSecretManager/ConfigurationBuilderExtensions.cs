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
    /// <param name="vaultNameOrUri">The Azure Key Vault name or URI. Handles whitespace and trailing slashes.</param>
    /// <param name="prefix">Optional prefix to filter secrets by. Secrets starting with this prefix will be loaded with the prefix stripped.</param>
    /// <param name="credential">The token credential to use for authentication.</param>
    /// <param name="options">The optional configuration options for Azure Key Vault.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddPrefixedAzureKeyVault(
        this IConfigurationBuilder configBuilder, 
        string vaultNameOrUri, 
        string? prefix, 
        TokenCredential credential, 
        AzureKeyVaultConfigurationOptions? options = null)
    {
        // Guard Clauses
        ArgumentNullException.ThrowIfNull(configBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(vaultNameOrUri);
    
        // Process inputs
        vaultNameOrUri = vaultNameOrUri.Trim().TrimEnd('/');
        prefix = prefix?.Trim() ?? string.Empty;
    
        var vaultUri = vaultNameOrUri.StartsWith("https://") 
            ? new Uri(vaultNameOrUri) 
            : new Uri($"https://{vaultNameOrUri}.vault.azure.net/");

        options ??= new AzureKeyVaultConfigurationOptions();
        options.Manager = new PrefixedAzureKeyVaultSecretManager(prefix);

        return configBuilder.AddAzureKeyVault(vaultUri, credential, options);
    }
}
