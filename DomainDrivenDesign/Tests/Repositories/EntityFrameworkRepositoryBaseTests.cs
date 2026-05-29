namespace Tests.Odin.DDD.Repositories;

public class EntityFrameworkRepositoryBaseTests : SupportedDatabasesTestBase
{
    public EntityFrameworkRepositoryBaseTests(AppFactory appFactory, DatabaseFixture database) 
        : base(appFactory, database)
    {
    }
    
    [Fact]
    public void FetchSingleById()
    {
        
    }
    
}