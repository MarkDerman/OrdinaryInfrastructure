namespace Odin.Domain;

/// <summary>
/// Marker interface to identify an aggregate root entity
/// in domain driven design language.
/// </summary>
public interface IAggregateRoot
{
}

/// <summary>
/// Marker interface to identify an aggregate root entity
/// that has a simple unique identifier, and typically a single column
/// primary key
/// </summary>
public interface IAggregateRoot<TId> : IAggregateRoot
{
   /// <summary>
   /// Unique identifier of the entity.
   /// Commonly int16\32\64, Guid or string.
   /// </summary>
   TId Id { get; }
}