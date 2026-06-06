using Tests.Odin.DDD.Repositories.Database;

namespace Tests.Odin.DDD.Repositories.EF.Fixtures;

public static class DatabaseTestFixtureSource
{
    public const string TestSuiteName = "Odin.DDD.Repos";
    public static IEnumerable<TestFixtureData> All
    {
        get { return SupportedDatabases; }
    }

    public static IEnumerable<TestFixtureData> SupportedDatabases
    {
        get
        {
            yield return Create(new SqlServerTestContainerAdapter(TestSuiteName,2025));
            yield return Create(new SqlServerTestContainerAdapter(TestSuiteName,2022));
            yield return Create(new PostgresTestContainerAdapter(TestSuiteName,16));
        }
    }
    
    private static TestFixtureData Create(IDatabaseTestContainerAdapter databaseTestContainer)
    {
        DatabaseTestContainerFixture testDatabaseFixture = new DatabaseTestContainerFixture(databaseTestContainer);

        return new TestFixtureData(testDatabaseFixture)
            .SetArgDisplayNames(databaseTestContainer.DatabaseProvider, databaseTestContainer.Image.FullName);
    }
}
