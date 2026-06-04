using Microsoft.EntityFrameworkCore;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public interface IDatabaseFixture : IAsyncDisposable
{
    string ConnectionString { get; }

    DbContextOptions<TestDatabaseContext> CreateOptions();

    ValueTask InitializeAsync();

    ValueTask ResetDatabaseAsync();
}
