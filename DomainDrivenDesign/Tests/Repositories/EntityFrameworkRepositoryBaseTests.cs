using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories;

public class EntityFrameworkRepositoryBaseTests : DatabaseTestBase
{
    public EntityFrameworkRepositoryBaseTests(AppFactory appFactory, SupportedDatabasesFixture database) 
        : base(appFactory, database)
    {
    }
    
    [Fact]
    public void FetchSingleById()
    {
        
    }
    
}