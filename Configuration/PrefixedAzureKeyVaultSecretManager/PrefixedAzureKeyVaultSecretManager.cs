using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly string _prefix;

    public PrefixKeyVaultSecretManager(string prefix)
    {
        _prefix = prefix;
    }

    public override bool Load(SecretProperties secret)
    {
        return secret.Name.StartsWith(_prefix);
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        // Strip prefix and map "--" to ":"
        var key = secret.Name.Substring(_prefix.Length);
        return key.Replace("--", ":");
    }
}