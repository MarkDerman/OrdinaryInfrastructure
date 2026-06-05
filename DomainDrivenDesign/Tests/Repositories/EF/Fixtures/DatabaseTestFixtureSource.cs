using Tests.Odin.DDD.Repositories.Database;

namespace Tests.Odin.DDD.Repositories.EF.Fixtures;

public static class DatabaseTestFixtureSource
{
    public static IEnumerable<TestFixtureData> All
    {
        get { return SupportedDatabases; }
    }

    public static IEnumerable<TestFixtureData> SupportedDatabases
    {
        get
        {
            yield return Create(new SqlServerDatabaseProviderAdapter());
            yield return Create(new PostgresDatabaseProviderAdapter(16));
        }
    }

    public static IEnumerable<TestFixtureData> SqlServer
    {
        get { yield return Create(new SqlServerDatabaseProviderAdapter()); }
    }

    public static IEnumerable<TestFixtureData> Postgres
    {
        get { yield return Create(new PostgresDatabaseProviderAdapter(16)); }
    }

    private static TestFixtureData Create(IDatabaseProviderAdapter databaseProvider)
    {
        TestDatabaseFixture testDatabaseFixture = new TestDatabaseFixture(databaseProvider);

        return new TestFixtureData(testDatabaseFixture)
            .SetArgDisplayNames(databaseProvider.DatabaseProvider);
    }
}
