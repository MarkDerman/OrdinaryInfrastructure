using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public interface IDatabaseFixture : IAsyncLifetime
{
    TestDatabaseContext CreateDbContext();

    string ProviderName { get; }
}