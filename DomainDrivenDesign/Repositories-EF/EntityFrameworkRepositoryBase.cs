using Microsoft.EntityFrameworkCore;

namespace Odin.DDD.Repositories
{
    /// <summary>
    /// Provides a base implementation for read-write repositories that use Entity Framework Core for data access.
    /// Exposes flexible FetchSingleAsync and FetchManyAsync query endpoints.
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    /// <typeparam name="TDbContext">The database context that must contain a DBSet of <typeparamref name="TAggregateRoot"/>
    /// We encapsulate save\commit under IUnitOfWork, in order that other commit-time aspects can be implemented,
    /// the most notable being domain event publishing.</typeparam>
    public abstract class EntityFrameworkRepositoryBase<TAggregateRoot, TDbContext>
        : EntityFrameworkReadOnlyRepositoryBase<TAggregateRoot, TDbContext>, IRepository<TAggregateRoot>
        where TDbContext : DbContext, IUnitOfWork
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// EntityFrameworkRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkRepositoryBase(TDbContext dbContext)
            : base(dbContext)
        {
        }
        
        /// <summary>
        /// EntityFrameworkRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbSet">The DbSet property</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkRepositoryBase(TDbContext dbContext, DbSet<TAggregateRoot> dbSet)
            :base( dbContext, dbSet)
        {
        }
        
        /// <summary>
        /// EntityFrameworkRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbSetName">The name of the DbSet property or EF named entity type.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkRepositoryBase(TDbContext dbContext, string dbSetName)
            : base(dbContext,dbSetName)
        {
        }

        /// <summary>
        /// Gets a value indicating whether queries should be created without EF change tracking.
        /// </summary>
        protected override bool UseNoTrackingQueries
        {
            get { return false; }
        }

        /// <inheritdoc />
        public IUnitOfWork UnitOfWork
        {
            get { return DbContext; }
        }

        /// <inheritdoc />
        public void Delete(TAggregateRoot entity)
        {
            DbSet.Remove(entity);
        }

        /// <inheritdoc />
        public void Add(TAggregateRoot entity)
        {
            DbSet.Add(entity);
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<TAggregateRoot> entities)
        {
            DbSet.AddRange(entities);
        }

        /// <inheritdoc />
        public virtual void Update(TAggregateRoot entity)
        {
            DbSet.Update(entity);
        }

    }
}
