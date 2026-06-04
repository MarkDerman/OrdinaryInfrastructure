using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public sealed class PostgresContainerFixture : IDatabaseFixture
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:18")
            .Build();

    public string ConnectionString => _container.GetConnectionString();

    public DbContextOptions<TestDatabaseContext> CreateOptions() 
    {
        return new DbContextOptionsBuilder<TestDatabaseContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }

    public ValueTask ResetDatabaseAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}