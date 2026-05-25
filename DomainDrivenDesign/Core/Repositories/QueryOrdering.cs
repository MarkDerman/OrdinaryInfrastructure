using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <summary>
/// Represents an ordering clause for a repository query.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
/// <param name="Expression">The ordering expression.</param>
/// <param name="Direction">The ordering direction.</param>
public sealed record QueryOrdering<TAggregateRoot>(
    Expression<Func<TAggregateRoot, object>> Expression,
    SortDirection Direction)
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// Creates an ascending ordering clause.
    /// </summary>
    /// <param name="expression">The ordering expression.</param>
    /// <returns>The ordering clause.</returns>
    public static QueryOrdering<TAggregateRoot> Ascending(Expression<Func<TAggregateRoot, object>> expression)
        => new(expression, SortDirection.Ascending);

    /// <summary>
    /// Creates a descending ordering clause.
    /// </summary>
    /// <param name="expression">The ordering expression.</param>
    /// <returns>The ordering clause.</returns>
    public static QueryOrdering<TAggregateRoot> Descending(Expression<Func<TAggregateRoot, object>> expression)
        => new(expression, SortDirection.Descending);
}
