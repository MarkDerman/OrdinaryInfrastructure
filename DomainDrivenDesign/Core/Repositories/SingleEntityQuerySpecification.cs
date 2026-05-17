using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <summary>
/// Defines a query to find a single entity in a repository.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
public class SingleEntityQuerySpecification<TAggregateRoot>
    : ISingleEntityQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// Default constructor for AbstractQuerySpecification
    /// </summary>
    /// <param name="criteria"></param>
    public SingleEntityQuerySpecification(Expression<Func<TAggregateRoot, bool>> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        Criteria = criteria;
    }

    /// <summary>
    /// Default constructor for AbstractQuerySpecification
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="includes"></param>
    public SingleEntityQuerySpecification(Expression<Func<TAggregateRoot, bool>> criteria,
        IEnumerable<Expression<Func<TAggregateRoot, object>>> includes)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentNullException.ThrowIfNull(includes);
        Criteria = criteria;
        _includes = includes.ToList();
    }

    /// <inheritdoc />
    public Expression<Func<TAggregateRoot, bool>> Criteria { get; }

    /// <inheritdoc />
    public IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? Includes
    {
        get { return _includes; }
    }

    private List<Expression<Func<TAggregateRoot, object>>>? _includes;

    /// <summary>
    /// Adds a query include
    /// </summary>
    /// <param name="includeExpression"></param>
    public void AddInclude(Expression<Func<TAggregateRoot, object>> includeExpression)
    {
        if (_includes == null) _includes = new List<Expression<Func<TAggregateRoot, object>>>();
        _includes.Add(includeExpression);
    }

}