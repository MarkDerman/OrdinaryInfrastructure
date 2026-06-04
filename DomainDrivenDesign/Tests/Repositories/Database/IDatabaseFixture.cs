using Microsoft.EntityFrameworkCore;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public interface IDatabaseFixture : IAsyncLifetime
{
    string ConnectionString { get; }

    DbContextOptions<TestDatabaseContext> CreateOptions();

    ValueTask ResetDatabaseAsync();
}