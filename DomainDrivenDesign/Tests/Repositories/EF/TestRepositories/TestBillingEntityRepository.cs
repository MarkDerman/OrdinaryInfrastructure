using Odin.DDD.Repositories;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestBillingEntityRepository
    : EntityFrameworkIdentityRepositoryBase<BillingEntity, int, TestDatabaseContext>
{
    public TestBillingEntityRepository(TestDatabaseContext dbContext)
        : base(dbContext)
    {
    }

    public Task<IReadOnlyList<BillingEntity>> ListAsync(IQuerySpecification<BillingEntity> specification)
    {
        return FetchManyAsync(specification);
    }

    public Task<BillingEntity?> SingleAsync(ISingleEntityQuerySpecification<BillingEntity> specification)
    {
        return FetchSingleAsync(specification);
    }

    public Task<IReadOnlyList<int>> ListIdsAsync(IQuerySpecification<BillingEntity> specification)
    {
        return FetchIdsAsync(specification);
    }
}
