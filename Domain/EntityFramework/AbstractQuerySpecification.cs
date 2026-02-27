using System.Linq.Expressions;

namespace Odin.Domain.EntityFramework;

/// <inheritdoc />
public abstract class AbstractQuerySpecification<T>(Expression<Func<T, bool>> criteria) 
    : IQuerySpecification<T>
{
    /// <inheritdoc />
    public Expression<Func<T, bool>> Criteria { get; } = criteria;
    
    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    
    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    
    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    
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
    protected void AddInclude(Expression<Func<T, object>> includeExpression) 
        => Includes.Add(includeExpression);

    /// <summary>
    /// Adds ordering ascending
    /// </summary>
    /// <param name="orderByExpression"></param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) 
        => OrderBy = orderByExpression;
    
    /// <summary>
    /// Adds ordering descending
    /// </summary>
    /// <param name="orderByExpression"></param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByExpression) 
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