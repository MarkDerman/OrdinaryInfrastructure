using System.Numerics;

namespace Odin.Domain;

/// <summary>
/// Marker interface to identify an 'aggregate root' entity
/// that has a primitive unique identifier property,
/// and typically a single column primary key.
/// (in domain driven design language).
/// </summary>
// ReSharper disable once TypeParameterCanBeVariant
public interface IIdentityAggregateRoot<TId> : IAggregateRoot
    where TId : struct, IEqualityOperators<TId, TId, bool>
{
    /// <summary>
    /// Unique identifier of the entity.
    /// Commonly int16\32\64, Guid or string.
    /// </summary>
    TId Id { get; }
}