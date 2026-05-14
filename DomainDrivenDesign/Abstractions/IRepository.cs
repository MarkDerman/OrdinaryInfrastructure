namespace Odin.DomainDrivenDesign
{
    /// <summary>
    /// Represents a repository for persisting entities to a data store.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    public interface IRepository<TAggregateRoot>  : IAsyncDisposable
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
        /// <param name="entities">The entities to add.</param>
        void AddRange(IEnumerable<TAggregateRoot> entities);

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
        /// Gets the unit of work, used to commit changes to the database,
        /// and possibly fire domain events.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }
    }
}