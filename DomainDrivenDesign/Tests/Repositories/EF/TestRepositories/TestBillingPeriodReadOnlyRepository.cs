using Odin.DDD.Repositories;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestBillingPeriodReadOnlyRepository
    : EntityFrameworkReadOnlyRepositoryBase<BillingPeriod, TestDatabaseContext>
{
    public TestBillingPeriodReadOnlyRepository(TestDatabaseContext dbContext)
        : base(dbContext)
    {
    }

    public Task<IReadOnlyList<BillingPeriod>> ListAsync(IQuerySpecification<BillingPeriod> specification)
    {
        return FetchManyAsync(specification);
    }

    public Task<BillingPeriod?> SingleAsync(ISingleEntityQuerySpecification<BillingPeriod> specification)
    {
        return FetchSingleAsync(specification);
    }
}
