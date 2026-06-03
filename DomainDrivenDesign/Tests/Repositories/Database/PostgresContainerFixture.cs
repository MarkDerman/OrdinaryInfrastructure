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

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public TestDatabaseContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDatabaseContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new TestDatabaseContext(options);
    }

    public string ProviderName
    {
        get { return "Postgres18"; }
    }
}