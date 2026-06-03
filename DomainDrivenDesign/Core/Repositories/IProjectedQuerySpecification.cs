using System.Linq.Expressions;

namespace Odin.DDD.Repositories;

/// <summary>
/// Extends IQuerySpecification to return a projection of TAggregateRoot.
/// This is often used to return only entity identifiers,
/// summaries of large objects in document databases, etc.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TProjection"></typeparam>
public interface IProjectedQuerySpecification<TAggregateRoot, TProjection>
    : IQuerySpecification<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// A projection to apply to TAggregateRoot.
    /// </summary>
    Expression<Func<TAggregateRoot, TProjection>> Projection { get; }
}