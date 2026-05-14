using System.Linq.Expressions;
using System.Numerics;

namespace Odin.DomainDrivenDesign;

/// <summary>
/// Represents a repository for persisting entities with a single column
/// primary key to a data store.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TIdentityType"></typeparam>
public interface ISqlRepository<TAggregateRoot, TIdentityType> : IRepository<TAggregateRoot>
    where TAggregateRoot : class, IIdentityAggregateRoot<TIdentityType>
    where TIdentityType : struct, IEqualityOperators<TIdentityType, TIdentityType, bool>
{
    /// <summary>
    /// Fetches an entity by its identifier, returning null if not found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TAggregateRoot?> FetchByIdAsync(TIdentityType id, CancellationToken ct = default);
        
    /// <summary>
    /// Fetches an entity by its identifier, returning null if not found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includes"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TAggregateRoot?> FetchByIdAsync(TIdentityType id, IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? includes
        , CancellationToken ct = default);

    /// <summary>
    /// Fetches entities from a list of their identifiers, returning an empty list if none found.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<IReadOnlyList<TAggregateRoot>> FetchManyByIdAsync(params ReadOnlySpan<TIdentityType> ids);
}