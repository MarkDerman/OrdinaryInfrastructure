using Azure.Core;
using Azure.Storage.Blobs;
using Odin.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddAzureBlobJsonConfiguration(this IConfigurationBuilder configBuilder,
        AzureBlobJsonConfigurationOptions options, Action<FileLoadExceptionContext>? onLoadException)
    {
        return configBuilder.Add(new AzureBlobJsonConfigurationSource(options, onLoadException));
    }

    public static IConfigurationBuilder AddAzureBlobJsonConfiguration(this IConfigurationBuilder configBuilder,
        AzureBlobJsonConfigurationOptions options, TokenCredential tokenCredential, Action<FileLoadExceptionContext>? onLoadException)
    {
        return configBuilder.Add(new AzureBlobJsonConfigurationSource(options, tokenCredential, onLoadException));
    }

    public static IConfigurationBuilder AddAzureBlobJsonConfiguration(this IConfigurationBuilder configBuilder,
        AzureBlobJsonConfigurationOptions options, BlobClient blobClient, Action<FileLoadExceptionContext>? onLoadException)
    {
        return configBuilder.Add(new AzureBlobJsonConfigurationSource(options, blobClient, onLoadException));
    }
}