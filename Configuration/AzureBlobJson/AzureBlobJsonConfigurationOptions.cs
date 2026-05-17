using Azure.Storage;
using System.ComponentModel.DataAnnotations;

namespace Odin.Configuration;

/// <summary>
/// Configuration options for accessing and reloading a JSON configuration file in Azure Blob storage.
/// </summary>
public record AzureBlobJsonConfigurationOptions
{
    /// <summary>
    /// Set to true to for the provider to effectively do nothing.
    /// </summary>
    public bool IsDisabled { get; init; }

    /// <summary>
    /// Frequency that the provider should reload.
    /// Default is 10 minutes.
    /// Set to null to disable reload.
    /// </summary>
    public double? ReloadPeriodSeconds { get; init; } = 600;

    /// <summary>
    /// e.g. "https://outgoingpaymentsstorage.blob.core.windows.net/dynamic-config/dynamic-config-LocalDev.json"
    /// </summary>
    [Required]
    public required Uri BlobUri { get; init; }

    public SharedKeyCredentialOptions? SharedKeyCredential { get; init; }
}

public record SharedKeyCredentialOptions
{
    public string AccountName { get; set; } = null!;
    public string AccountKey { get; set; } = null!;

    public StorageSharedKeyCredential GetCredential()
    {
        return new StorageSharedKeyCredential(AccountName, AccountKey);
    }
}