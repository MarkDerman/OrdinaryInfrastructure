using Odin.DDD.Repositories;
using System.Linq.Expressions;
using Tests.Odin.DDD.Repositories.TestDomain;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestProjectedBillingEntitySpecification
    : QuerySpecificationBase<BillingEntity>, IProjectedQuerySpecification<BillingEntity, BillingEntitySummary>
{
    public TestProjectedBillingEntitySpecification(Expression<Func<BillingEntity, bool>> criteria)
    {
        ApplyCriteria(criteria);
    }

    public Expression<Func<BillingEntity, BillingEntitySummary>> Projection
    {
        get { return billingEntity => new BillingEntitySummary(billingEntity.Id, billingEntity.Name); }
    }
}

internal sealed record BillingEntitySummary(int Id, string Name);
