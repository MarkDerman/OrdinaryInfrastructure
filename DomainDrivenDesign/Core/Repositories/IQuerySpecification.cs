using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <summary>
/// Represents a repository query specification for filter criteria, preloading includes,
/// ordering and pagination for a repository query that can return multiple entities.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
public interface IQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// Filter criteria (Where clause)
    /// Can be optional for querying all entities from a repository.
    /// </summary>
    Expression<Func<TAggregateRoot, bool>>? Criteria { get; }

    /// <summary>
    /// Ordering clauses for the query.
    /// </summary>
    IReadOnlyList<QueryOrdering<TAggregateRoot>>? Orderings { get; }

    /// <summary>
    /// Whether to retrieve a specific page in the results of the query.
    /// </summary>
    Pagination? Page { get; }
    
    /// <summary>
    /// Eager loading (Include clauses)
    /// </summary>
    IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? Includes { get; }

}
