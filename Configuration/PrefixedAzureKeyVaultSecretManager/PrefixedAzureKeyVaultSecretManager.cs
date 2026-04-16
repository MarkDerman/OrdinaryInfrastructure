using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace Odin.Configuration;

/// <summary>
/// Custom manager to handle shared Key Vaults where secrets are prefixed with e.g. {Project}-{Environment}-
/// </summary>
public class PrefixedAzureKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly string _prefix;

    /// <summary>
    /// Use custom prefix to handle shared Key Vaults where secrets are prefixed with e.g. {Project}-{Environment}-
    /// </summary>
    /// <param name="prefix"></param>
    public PrefixedAzureKeyVaultSecretManager(string prefix)
    {
        _prefix = prefix;
    }
    
    /// <summary>
    /// Only load secrets that start with the prefix
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    public override bool Load(SecretProperties secret)
    {
        return secret.Name.StartsWith(_prefix);
    }

    /// <summary>
    /// Strip the prefix and replace "--" with ":"
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    public override string GetKey(KeyVaultSecret secret)
    {
        // Strip prefix and map "--" to ":"
        var key = secret.Name.Substring(_prefix.Length);
        return key.Replace("-", ":");
    }
}