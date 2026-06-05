namespace Odin.DDD.Repositories
{
    /// <summary>
    /// Represents a read-only repository for aggregate roots.
    /// Concrete repository contracts should expose domain-specific read operations.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    public interface IReadOnlyRepository<TAggregateRoot> : IAsyncDisposable
        where TAggregateRoot : class, IAggregateRoot
    {
    }
}
