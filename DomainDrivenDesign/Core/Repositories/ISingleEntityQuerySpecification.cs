using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <summary>
/// Represents a repository query specification (filter criteria and preloading includes)
/// for a query that can only return 0 or 1 entities from the repository.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
public interface ISingleEntityQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// The mandatory filter criteria (Where clause)
    /// </summary>
    Expression<Func<TAggregateRoot, bool>> Criteria { get; }

    /// <summary>
    /// Eager loading (Include clauses)
    /// </summary>
    IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? Includes { get; }

}