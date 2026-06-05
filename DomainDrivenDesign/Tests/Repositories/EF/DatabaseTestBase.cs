using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Odin.System;
using Respawn;
using Tests.Odin.DDD.Repositories.Database;

namespace Tests.Odin.DDD.Repositories.EF;

public abstract class DatabaseTestBase : IAsyncDisposable
{
    private readonly IServiceScope _scope;
    private readonly AppFactory _appFactory;
    protected readonly TestDatabaseFixture TestDatabase;

    protected DatabaseTestBase(AppFactory appFactory, TestDatabaseFixture testDatabase)
    {
        _appFactory = appFactory;
        _scope = _appFactory.Services.CreateScope();
        TestDatabase = testDatabase;
    }

    public IServiceScope TestScope => _scope;

    public async ValueTask InitializeAsync()
    {
        // Ensure database is reset before each test
        await TestDatabase.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}
