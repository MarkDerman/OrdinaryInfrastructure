using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <summary>
/// Base class that can be used for query specification implementations that can retrieve many results .
/// Typical usage is to apply query criteria, orderings, includes, and pagination
/// in the constructor of the derived specification class.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
public abstract class QuerySpecificationBase<TAggregateRoot>
    : IQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
   
    /// <summary>
    /// Creates a query specification with no criteria. This will return all entities.
    /// </summary>
    protected QuerySpecificationBase()
    {
    }

    /// <inheritdoc />
    public Expression<Func<TAggregateRoot, bool>>? Criteria => _criteria;
    private Expression<Func<TAggregateRoot, bool>>? _criteria;
    
    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? Includes => _includes;
    private List<Expression<Func<TAggregateRoot, object>>>? _includes;

    /// <inheritdoc />
    public IReadOnlyList<QueryOrdering<TAggregateRoot>>? Orderings => _orderings;
    private List<QueryOrdering<TAggregateRoot>>? _orderings;

    /// <inheritdoc />
    public Pagination? Page { get; private set; }

    
    /// <summary>
    /// Adds a query include
    /// </summary>
    /// <param name="criteria"></param>
    protected void ApplyCriteria(Expression<Func<TAggregateRoot, bool>> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        _criteria = criteria;
    }
    
    /// <summary>
    /// Adds a query include
    /// </summary>
    /// <param name="includeExpression"></param>
    protected void AddInclude(Expression<Func<TAggregateRoot, object>> includeExpression)
    {
        if (_includes == null) _includes = new List<Expression<Func<TAggregateRoot, object>>>();
        _includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds ordering ascending to the ordering layers
    /// </summary>
    /// <param name="orderByExpression"></param>
    protected void AddOrderByAscending(Expression<Func<TAggregateRoot, object>> orderByExpression)
        => AddOrdering(orderByExpression, SortDirection.Ascending);

    /// <summary>
    /// Adds ordering descending
    /// </summary>
    /// <param name="orderByExpression"></param>
    protected void ApplyOrderByDescending(Expression<Func<TAggregateRoot, object>> orderByExpression)
        => AddOrdering(orderByExpression, SortDirection.Descending);

    /// <summary>
    /// Adds subsequent ordering ascending.
    /// </summary>
    /// <param name="thenByExpression"></param>
    protected void ApplyThenByAscending(Expression<Func<TAggregateRoot, object>> thenByExpression)
        => AddOrdering(thenByExpression, SortDirection.Ascending);

    /// <summary>
    /// Adds subsequent ordering descending.
    /// </summary>
    /// <param name="thenByExpression"></param>
    protected void ApplyThenByDescending(Expression<Func<TAggregateRoot, object>> thenByExpression)
        => AddOrdering(thenByExpression, SortDirection.Descending);

    /// <summary>
    /// Applies ordering clauses.
    /// </summary>
    /// <param name="orderings"></param>
    protected void ApplyOrdering(params QueryOrdering<TAggregateRoot>[] orderings)
    {
        ArgumentNullException.ThrowIfNull(orderings);

        foreach (QueryOrdering<TAggregateRoot> ordering in orderings)
        {
            AddOrdering(ordering);
        }
    }

    private void AddOrdering(Expression<Func<TAggregateRoot, object>> expression, SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(expression);

        AddOrdering(new QueryOrdering<TAggregateRoot>(expression, direction));
    }

    private void AddOrdering(QueryOrdering<TAggregateRoot> ordering)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(ordering.Expression);

        if (_orderings == null) _orderings = new List<QueryOrdering<TAggregateRoot>>();
        _orderings.Add(ordering);
    }

    /// <summary>
    /// Applies pagination options.
    /// </summary>
    /// <param name="page"></param>
    protected void ApplyPaging(Pagination page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Page = page;
    }

}
