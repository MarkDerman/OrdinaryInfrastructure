using Microsoft.EntityFrameworkCore;
using Tests.Odin.DDD.Repositories.Database;
using Tests.Odin.DDD.Repositories.EF.Fixtures;

namespace Tests.Odin.DDD.Repositories.EF;

public abstract class DatabaseTestBase
{
    protected readonly DatabaseTestContainerFixture TestDatabase;

    protected DatabaseTestBase(DatabaseTestContainerFixture testDatabase)
    {
        ArgumentNullException.ThrowIfNull(testDatabase);
        TestDatabase = testDatabase;
    }

    [OneTimeSetUp]
    public async Task InitializeDatabaseAsync()
    {
        await TestDatabase.InitializeAsync();
    }

    [SetUp]
    public async Task ResetDatabaseAsync()
    {
        await TestDatabase.ResetDatabaseAsync();
    }

    [OneTimeTearDown]
    public async Task DisposeDatabaseAsync()
    {
        await TestDatabase.DisposeAsync();
    }

    protected TestDatabaseContext CreateContext()
    {
        DbContextOptionsBuilder<TestDatabaseContext> optionsBuilder = new DbContextOptionsBuilder<TestDatabaseContext>();
        TestDatabase.DatabaseTestContainer.ConfigureDbContext(optionsBuilder, TestDatabase.ConnectionString);
        return new TestDatabaseContext(optionsBuilder.Options);
    }

    protected Seeder CreateSeeder(TestDatabaseContext context)
    {
        return new Seeder(context);
    }
}
