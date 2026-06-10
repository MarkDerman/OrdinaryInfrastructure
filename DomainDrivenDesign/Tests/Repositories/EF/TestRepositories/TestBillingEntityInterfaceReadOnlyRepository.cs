using Odin.DDD.Repositories;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestBillingEntityInterfaceReadOnlyRepository
    : EntityFrameworkReadOnlyRepositoryBase<IBillingEntity, TestDatabaseContext>
{
    public TestBillingEntityInterfaceReadOnlyRepository(TestDatabaseContext dbContext)
        : base(dbContext, nameof(TestDatabaseContext.BillingEntities))
    {
    }

    public Task<IReadOnlyList<IBillingEntity>> ListAsync(IQuerySpecification<IBillingEntity> specification)
    {
        return FetchManyAsync(specification);
    }
}
