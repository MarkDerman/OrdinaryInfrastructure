## Odin.Configuration.AzureKeyVault

[![NuGet](https://img.shields.io/nuget/v/Odin.Configuration.AzureKeyVault.svg)](https://www.nuget.org/packages/Odin.Configuration.AzureKeyVault)            ![Nuget](https://img.shields.io/nuget/dt/Odin.Configuration.AzureKeyVault)

## PrefixedAzureKeyVaultSecretManager

Supports loading into IConfiguration from AzureKeyVault, optionally stripping a 'prefix' off each keyvault secret's name.

This is often used to store secrets for multiple Environments and\or Systems in 1 keyvault.

## Adding to IConfiguration in Application Startup

1 - Add configuration

```json
{
    "AzureKeyVault": {
        "Enabled": false,
        "VaultName": "MyVault",
        "VaultUri": "https://MyVault.vault.azure.net", // either of the 'VaultXXX' properties can be used.
        "TenantId": "XXXX",
        "ClientId": "YYYY",
        "Secret": "ZZZZ~~~~",
        "Prefix": "Treasury-QA-"
    }
}
```

2 - Add package references to Odin.Email, and in this case Odin.Email.Mailgun

3 - Add IEmailSender to DI in your startup code...

```csharp
    builder.Services.AddOdinEmailSending();
```
