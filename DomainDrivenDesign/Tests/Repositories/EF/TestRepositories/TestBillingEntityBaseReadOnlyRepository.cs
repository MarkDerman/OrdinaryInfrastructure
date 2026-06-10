using Odin.DDD.Repositories;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestBillingEntityBaseReadOnlyRepository
    : EntityFrameworkReadOnlyRepositoryBase<BillingEntityBase, TestDatabaseContext>
{
    public TestBillingEntityBaseReadOnlyRepository(TestDatabaseContext dbContext)
        : base(dbContext, nameof(TestDatabaseContext.BillingEntities))
    {
    }

    public Task<IReadOnlyList<BillingEntityBase>> ListAsync(IQuerySpecification<BillingEntityBase> specification)
    {
        return FetchManyAsync(specification);
    }
}
