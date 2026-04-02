using System.Linq.Expressions;

namespace Odin.Domain;

/// <summary>
/// Represents a specification for filter criteria, preloading includes,
/// ordering and pagination for a repository query.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
public interface IQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// The filter criteria (Where clause)
    /// </summary>
    Expression<Func<TAggregateRoot, bool>> Criteria { get; }

    /// <summary>
    /// Eager loading (Include clauses)
    /// </summary>
    List<Expression<Func<TAggregateRoot, object>>> Includes { get; }

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