using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public sealed class SqlServerContainerFixture : IDatabaseFixture
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
    
    public ValueTask ResetDatabaseAsync()
    {
        return ValueTask.CompletedTask;
    }

    public DbContextOptions<TestDatabaseContext> CreateOptions() 
    {
        return new DbContextOptionsBuilder<TestDatabaseContext>()
            .UseSqlServer(ConnectionString)
            .Options;
    }
}