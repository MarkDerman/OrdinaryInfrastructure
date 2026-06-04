using Microsoft.Extensions.DependencyInjection;
using Tests.Odin.DDD.Repositories.Database;

namespace Tests.Odin.DDD.Repositories.EF;

public abstract class DatabaseTestBase : IAsyncDisposable
{
    private readonly IServiceScope _scope;
    private readonly AppFactory _appFactory;
    protected readonly IDatabaseFixture Database;

    protected DatabaseTestBase(AppFactory appFactory, IDatabaseFixture database)
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
