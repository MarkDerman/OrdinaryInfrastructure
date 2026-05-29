using Microsoft.Extensions.DependencyInjection;

namespace Tests.Odin.DDD.Repositories.EF;

[Collection(nameof(SupportedDatabasesCollection))]
public abstract class DatabaseTestBase : IClassFixture<AppFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly AppFactory _appFactory;
    protected readonly SupportedDatabasesFixture Database;

    protected DatabaseTestBase(AppFactory appFactory, SupportedDatabasesFixture database)
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