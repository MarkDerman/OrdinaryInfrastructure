using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Odin.Email;

public sealed class TestApplicationFactory : IDisposable, IAsyncDisposable
{
    private readonly Lazy<WebApplication> _application = new(() => Program.BuildApplication());

    public IServiceProvider Services => _application.Value.Services;

    public IConfiguration GetConfiguration()
    {
        return Services.GetRequiredService<IConfiguration>();
    }

    public void Dispose()
    {
        if (_application.IsValueCreated)
        {
            _application.Value.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_application.IsValueCreated)
        {
            await _application.Value.DisposeAsync();
        }
    }
}
