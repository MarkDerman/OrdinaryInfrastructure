using Odin.DDD.Repositories;
using System.Linq.Expressions;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestBillingEntityReadOnlyRepository
    : EntityFrameworkReadOnlyIdentityRepositoryBase<BillingEntity, int, TestDatabaseContext>
{
    public TestBillingEntityReadOnlyRepository(TestDatabaseContext dbContext)
        : base(dbContext)
    {
    }

    public Task<IReadOnlyList<BillingEntity>> ListAsync(IQuerySpecification<BillingEntity> specification)
    {
        return FetchManyAsync(specification);
    }

    public Task<IReadOnlyList<TProjection>> ListAsync<TProjection>(
        IQuerySpecification<BillingEntity> specification,
        Expression<Func<BillingEntity, TProjection>> projection)
    {
        return FetchManyAsync(specification, projection);
    }

    public Task<IReadOnlyList<TProjection>> ListAsync<TProjection>(
        IProjectedQuerySpecification<BillingEntity, TProjection> specification)
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
