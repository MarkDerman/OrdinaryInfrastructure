using Odin.DDD;
using Odin.DDD.Repositories;
using System.Linq.Expressions;

namespace Tests.Odin.DDD.Repositories.EF.TestRepositories;

internal sealed class TestQuerySpecification<TAggregateRoot> : QuerySpecificationBase<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    public TestQuerySpecification()
    {
    }

    public TestQuerySpecification(Expression<Func<TAggregateRoot, bool>> criteria)
    {
        ApplyCriteria(criteria);
    }

    public TestQuerySpecification<TAggregateRoot> Include(Expression<Func<TAggregateRoot, object>> includeExpression)
    {
        AddInclude(includeExpression);
        return this;
    }

    public TestQuerySpecification<TAggregateRoot> OrderAscending(
        Expression<Func<TAggregateRoot, object>> orderByExpression)
    {
        AddOrderByAscending(orderByExpression);
        return this;
    }

    public TestQuerySpecification<TAggregateRoot> OrderDescending(
        Expression<Func<TAggregateRoot, object>> orderByExpression)
    {
        ApplyOrderByDescending(orderByExpression);
        return this;
    }

    public TestQuerySpecification<TAggregateRoot> ThenAscending(
        Expression<Func<TAggregateRoot, object>> thenByExpression)
    {
        ApplyThenByAscending(thenByExpression);
        return this;
    }

    public TestQuerySpecification<TAggregateRoot> ThenDescending(
        Expression<Func<TAggregateRoot, object>> thenByExpression)
    {
        ApplyThenByDescending(thenByExpression);
        return this;
    }

    public TestQuerySpecification<TAggregateRoot> ApplyPage(int pageNumber, int pageSize)
    {
        ApplyPaging(new Pagination(pageNumber, pageSize));
        return this;
    }
}
