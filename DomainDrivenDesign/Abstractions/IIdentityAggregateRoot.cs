using System.Numerics;

namespace Odin.DomainDrivenDesign;

/// <summary>
/// Marker interface to identify an 'aggregate root' entity
/// that has a primitive unique identifier property called 'Id'.
/// And in 99% of cases is backed by a repository table with
/// a single primary key column.
/// Id must be a .NET struct type, such as int16\32\64 or Guid (but not string).
/// </summary>
// ReSharper disable once TypeParameterCanBeVariant
public interface IIdentityAggregateRoot<TId> : IAggregateRoot
    where TId : struct, IEqualityOperators<TId, TId, bool>
{
    /// <summary>
    /// Unique identifier of the aggregate root entity,
    /// where the identity type is a C# struct type.
    /// Commonly int16\32\64 or Guid (but notably not string).
    /// </summary>
    TId Id { get; }
}