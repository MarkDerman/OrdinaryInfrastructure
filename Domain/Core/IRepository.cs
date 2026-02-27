namespace Odin.Domain
{
    /// <summary>
    /// Represents a repository for persisting entities to a data store.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    public interface IRepository<TAggregateRoot> 
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// Adds an entity to the context but does not persist it to the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(TAggregateRoot entity);

        /// <summary>
        /// Adds a range of entities to the context but does not persist them to the database.
        /// </summary>
        /// <param name="entity">The entities to add.</param>
        void AddRange(IEnumerable<TAggregateRoot> entity);

        /// <summary>
        /// Updates an entity in the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(TAggregateRoot entity);

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(TAggregateRoot entity);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TAggregateRoot?> FetchAsync(IQuerySpecification<TAggregateRoot> spec, 
            CancellationToken ct = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<TAggregateRoot>> FetchManyAsync(
            IQuerySpecification<TAggregateRoot> spec, CancellationToken ct = default);

        /// <summary>
        /// Gets the unit of work, used to commit changes to the database.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }
    }

    /// <summary>
    /// Represents a repository for persisting entities with a single column
    /// primary key to a data store.
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IRepository<TAggregateRoot, TId> : IRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot<TId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken ct = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IReadOnlyList<TAggregateRoot>> FetchManyByIdAsync(params ReadOnlySpan<TId> ids);
    }
}