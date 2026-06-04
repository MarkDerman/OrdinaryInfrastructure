using Tests.Odin.DDD.Repositories.Database;

namespace Tests.Odin.DDD.Repositories.EF;

public abstract class EntityFrameworkRepositoryBaseTests : DatabaseTestBase
{
    protected EntityFrameworkRepositoryBaseTests(AppFactory appFactory, IDatabaseFixture database) 
        : base(appFactory, database)
    {
    }
    
}