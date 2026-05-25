using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <inheritdoc />
public class QuerySpecification<TAggregateRoot>
    : IQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// At a minimum, Criteria are needed for a QuerySpecification (even if no cr
    /// </summary>
    /// <param name="criteria"></param>
    public QuerySpecification(Expression<Func<TAggregateRoot, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Creates a query specification with no criteria. THis will return all entities.
    /// </summary>
    public QuerySpecification()
    {
    }

    /// <inheritdoc />
    public Expression<Func<TAggregateRoot, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? Includes => _includes;
    private List<Expression<Func<TAggregateRoot, object>>>? _includes;

    /// <inheritdoc />
    public Expression<Func<TAggregateRoot, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<TAggregateRoot, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public int Take { get; private set; }

    /// <inheritdoc />
    public int Skip { get; private set; }

    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Adds a query include
    /// </summary>
    /// <param name="includeExpression"></param>
    protected void AddCriteria(Expression<Func<TAggregateRoot, object>> includeExpression)
    {
        if (_includes == null) _includes = new List<Expression<Func<TAggregateRoot, object>>>();
        _includes.Add(includeExpression);
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
    /// Adds ordering ascending
    /// </summary>
    /// <param name="orderByExpression"></param>
    protected void ApplyOrderBy(Expression<Func<TAggregateRoot, object>> orderByExpression)
        => OrderBy = orderByExpression;

    /// <summary>
    /// Adds ordering descending
    /// </summary>
    /// <param name="orderByExpression"></param>
    protected void ApplyOrderByDescending(Expression<Func<TAggregateRoot, object>> orderByExpression)
        => OrderByDescending = orderByExpression;

    /// <summary>
    /// Applies pagination options.
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}