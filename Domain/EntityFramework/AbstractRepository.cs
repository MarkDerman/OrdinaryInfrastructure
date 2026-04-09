using Microsoft.EntityFrameworkCore;

namespace Odin.Domain.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class AbstractRepository<TAggregateRoot, TDbContext> : IRepository<TAggregateRoot>, IDisposable
        where TDbContext : DbContext, IUnitOfWork
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// The context for database operations
        /// </summary>
        protected readonly TDbContext DbContext;
        
        /// <summary>
        /// The DbSet for the aggregate root type
        /// </summary>
        protected readonly DbSet<TAggregateRoot> DbSet;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AbstractRepository(TDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = DbContext.Set<TAggregateRoot>();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TAggregateRoot>> FetchManyAsync(IQuerySpecification<TAggregateRoot> querySpecification, 
            CancellationToken ct = default)
        {
            return await ApplySpecification(querySpecification).ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<TAggregateRoot?> FetchAsync(IQuerySpecification<TAggregateRoot> querySpecification, CancellationToken ct = default)
        {
            return await ApplySpecification(querySpecification).FirstOrDefaultAsync(ct);
        }

        private IQueryable<TAggregateRoot> ApplySpecification(IQuerySpecification<TAggregateRoot> spec)
        {
            IQueryable<TAggregateRoot> query = DbSet.AsQueryable();

            // Apply includes
            foreach (var include in spec.Includes)
            {
                query = query.Include(include);
            }

            // Apply criteria
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // Apply ordering
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            // Apply paging
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }

        /// <inheritdoc />
        public IUnitOfWork UnitOfWork => DbContext;

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
        
        /// <inheritdoc />
        public void Dispose()
        {
            DbContext.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }

    }
}