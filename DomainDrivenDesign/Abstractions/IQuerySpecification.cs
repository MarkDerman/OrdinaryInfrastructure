using System.Linq.Expressions;

namespace Odin.DomainDrivenDesign;

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
    /// Eager loading (Include clauses)
    /// </summary>
    IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? Includes { get; }
    
    /// <summary>
    /// Ordering ascending
    /// </summary>
    Expression<Func<TAggregateRoot, object>>? OrderBy { get; }

    /// <summary>
    /// Ordering descending
    /// </summary>
    Expression<Func<TAggregateRoot, object>>? OrderByDescending { get; }

    /// <summary>
    /// Pagination - Take
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Pagination - Skip
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Whether pagination is enabled
    /// </summary>
    bool IsPagingEnabled { get; }
}