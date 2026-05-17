using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Runtime.ExceptionServices;

namespace Odin.Configuration;

/// <summary>
/// Provides configuration key-value pairs that are obtained from a JSON file located in Azure Blob storage.
/// </summary>
public class AzureBlobJsonConfigurationProvider : JsonConfigurationProvider
{
    private readonly Timer? _timer;
    private readonly bool _isDisabled;

    public AzureBlobJsonConfigurationProvider(AzureBlobJsonConfigurationSource source) : base(source)
    {
        _isDisabled = source.Options.IsDisabled;
        if (_isDisabled) return;
        if (source.Options.ReloadPeriodSeconds.HasValue && source.Options.ReloadPeriodSeconds.Value > 0)
        {
            TimeSpan period = TimeSpan.FromSeconds(source.Options.ReloadPeriodSeconds.Value);
            _timer = new Timer(AutoReload, null, period, period);
        }
    }

    private BlobClient GetBlobClient()
    {
        if (Source is not AzureBlobJsonConfigurationSource blobSource)
        {
            throw new ApplicationException($"{nameof(AzureBlobJsonConfigurationProvider)}.{nameof(Source)} is {Source.GetType()}, not {nameof(AzureBlobJsonConfigurationSource)}");
        }

        return blobSource.GetBlobClient();
    }

    private int _isAutoReloading = 0;

    private void AutoReload(object? _)
    {
        if (Interlocked.Exchange(ref _isAutoReloading, 1) == 1)
        {
            return;
        }

        try
        {
            Load();
        }
        finally
        {
            Interlocked.Exchange(ref _isAutoReloading, 0);
        }
    }

    public override void Load()
    {
        if (_isDisabled) return;
        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            HandleException(ExceptionDispatchInfo.Capture(ex));
        }
    }

    private readonly SemaphoreSlim _loadAsyncSemaphore = new(1);

    private ETag? _mostRecentEtag;

    private async Task LoadAsync()
    {
        await _loadAsyncSemaphore.WaitAsync();
        try
        {
            BlobClient client = GetBlobClient();

            Response<BlobProperties>? attributes = await client.GetPropertiesAsync();

            if (attributes.HasValue && attributes.Value.ETag == _mostRecentEtag)
            {
                return;
            }

            using MemoryStream memStream = new MemoryStream();

            await client.DownloadToAsync(memStream);

            memStream.Position = 0;

            Load(memStream);

            _mostRecentEtag = attributes.HasValue ? attributes.Value.ETag : null;

            OnReload();
        }
        finally
        {
            _loadAsyncSemaphore.Release();
        }
    }

    // Copied from Microsoft.Extensions.Configuration.FileConfigurationProvider
    private void HandleException(ExceptionDispatchInfo info)
    {
        bool ignoreException = false;
        if (Source.OnLoadException != null)
        {
            FileLoadExceptionContext exceptionContext = new FileLoadExceptionContext
            {
                Provider = this,
                Exception = info.SourceException
            };
            Source.OnLoadException.Invoke(exceptionContext);
            ignoreException = exceptionContext.Ignore;
        }

        if (!ignoreException)
        {
            info.Throw();
        }
    }
}