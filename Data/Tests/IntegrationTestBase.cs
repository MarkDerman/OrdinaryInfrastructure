using Microsoft.Extensions.DependencyInjection;
using Tests.Odin.Data.Database;

namespace Tests.Odin.Data;

[Collection(nameof(DatabaseCollection))]
public abstract class IntegrationTestBase : IClassFixture<AppFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly AppFactory _appFactory;
    protected readonly DatabaseFixture Database;

    protected IntegrationTestBase(AppFactory appFactory, DatabaseFixture database)
    {
        _appFactory = appFactory;
        _scope = _appFactory.Services.CreateScope();
        Database = database;
    }

    public IServiceScope TestScope => _scope;

    public async ValueTask InitializeAsync()
    {
        await Database.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}