## Odin.Configuration.AzureKeyVault

[![NuGet](https://img.shields.io/nuget/v/Odin.Configuration.AzureKeyVault.svg)](https://www.nuget.org/packages/Odin.Configuration.AzureKeyVault)            ![Nuget](https://img.shields.io/nuget/dt/Odin.Configuration.AzureKeyVault)

## PrefixedAzureKeyVaultSecretManager

Supports loading into IConfiguration from AzureKeyVault, optionally stripping a 'prefix' off each keyvault secret's name.

This is often used to store secrets for multiple Environments and\or Systems in 1 keyvault.

## Adding to IConfiguration in Application Startup

1 - Add configuration from json \ environment variables \ configmaps.

```json
{
    "AzureKeyVault": {
        "Enabled": true, // defaults to true if missing or invalid.
        "VaultName": "MyVault",
        "VaultUri": "https://MyVault.vault.azure.net", // either of the 'VaultXXX' properties can be used.
        "TenantId": "XXXX", // required
        "ClientId": "YYYY", // required
        "Secret": "ZZZZ~~~~", // required
        "Prefix": "Treasury-QA-" // optional~~~~
    }
}
```

2 - Add Nuget package Odin.Configuration.AzureKeyVault to your project.

3 - After adding whatever configuration sources contain the 'AzureKeyVault' configuration needed in 1 above, 
load your prefixed configuration from Azure KeyVault

```csharp
    builder.Configuration.AddOdinPrefixedAzureKeyVault("AzureKeyVault");
    // or
    builder.Configuration.AddOdinPrefixedAzureKeyVault(IConfigurationSection configSection);
    // or
    builder.Configuration.AddOdinPrefixedAzureKeyVault(string keyVaultName, string prefix, TokenCredential creds);
```
