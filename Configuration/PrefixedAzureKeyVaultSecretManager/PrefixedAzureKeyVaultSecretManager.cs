using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace Odin.Configuration;

/// <summary>
/// Loads secrets from shared Key Vaults when secret names start with an application/environment prefix.
/// </summary>
public class PrefixedAzureKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly string _prefix;

    /// <summary>
    /// Creates a manager for secrets prefixed with values such as <c>{Project}-{Environment}-</c>.
    /// </summary>
    /// <param name="prefix">The secret name prefix to load and strip from configuration keys.</param>
    public PrefixedAzureKeyVaultSecretManager(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        _prefix = prefix.Trim();
    }

    /// <summary>
    /// Loads only secrets whose names start with the configured prefix.
    /// </summary>
    /// <param name="secret">The Key Vault secret metadata.</param>
    /// <returns><c>true</c> when the secret name starts with the prefix; otherwise <c>false</c>.</returns>
    public override bool Load(SecretProperties secret)
    {
        return secret.Name.StartsWith(_prefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Strips the prefix and maps every hyphen in the remaining secret name to a configuration key delimiter.
    /// </summary>
    /// <param name="secret">The Key Vault secret.</param>
    /// <returns>The configuration key produced from the secret name.</returns>
    public override string GetKey(KeyVaultSecret secret)
    {
        var key = secret.Name.Substring(_prefix.Length);
        return key.Replace("-", ":");
    }
}
