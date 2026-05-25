using Microsoft.EntityFrameworkCore;

namespace Odin.DDD.Repositories
{
    /// <summary>
    /// Provides a base implementation for repositories that use Entity Framework Core for data access.
    /// Exposes flexible FetchSingleAsync and FetchManyAsync query endpoints.
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    /// <typeparam name="TDbContext">The database context that must contain a DBSet of <typeparamref name="TAggregateRoot"/>
    /// We encapsulate save\commit under IUnitOfWork, in order that other commit-time aspects can be implemented,
    /// the most notable being domain event publishing.</typeparam>
    public abstract class EntityFrameworkRepositoryBase<TAggregateRoot, TDbContext> : IRepository<TAggregateRoot>, IDisposable
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
        protected EntityFrameworkRepositoryBase(TDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            DbContext = dbContext;
            // If TAggregateRoot is a base type of the actual implementation type in the database context
            // then this exception will be thrown which is no good.
            // if (DbContext.Model.FindEntityType(typeof(TAggregateRoot)) == null)
            // {
            //     throw new InvalidOperationException(
            //         $"The DbContext does not contain a DbSet for aggregate root type {typeof(TAggregateRoot).FullName}.");
            // }
            DbSet = DbContext.Set<TAggregateRoot>();
        }

        /// <summary>
        /// Queries the repository based on the query specification.
        /// </summary>
        /// <param name="fetchManySpec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected async Task<IReadOnlyList<TAggregateRoot>> FetchManyAsync(
            IQuerySpecification<TAggregateRoot> fetchManySpec, CancellationToken ct = default)
        {
            return await AsQueryable(fetchManySpec).ToListAsync(ct);
        }

        /// <summary>
        /// Queries for a single entity from the query spec, returning null if
        /// no result returned from the repository.
        /// Throws an exception if multiple entities are found.
        /// </summary>
        /// <param name="fetchOneSpec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected async Task<TAggregateRoot?> FetchSingleAsync(ISingleEntityQuerySpecification<TAggregateRoot> fetchOneSpec,
            CancellationToken ct = default)
        {
            var results = await AsSingleResultQueryable(fetchOneSpec).ToListAsync(ct);
            if (results.Count > 1)
            {
                throw new InvalidOperationException($"Expected single result, but found {results.Count}.");
            }
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Applies the query criteria and eager-loading includes, if any.
        /// </summary>
        /// <param name="singleEntityQuerySpecification"></param>
        /// <returns></returns>
        protected IQueryable<TAggregateRoot> AsSingleResultQueryable(
            ISingleEntityQuerySpecification<TAggregateRoot> singleEntityQuerySpecification)
        {
            ArgumentNullException.ThrowIfNull(singleEntityQuerySpecification);
            IQueryable<TAggregateRoot> query = DbSet.AsQueryable()
                .Where(singleEntityQuerySpecification.Criteria);

            // Apply includes
            if (singleEntityQuerySpecification.Includes != null)
            {
                foreach (var include in singleEntityQuerySpecification.Includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        protected IQueryable<TAggregateRoot> AsQueryable(IQuerySpecification<TAggregateRoot> spec)
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
            if (spec.Orderings != null)
            {
                IOrderedQueryable<TAggregateRoot>? orderedQuery = null;

                for (int i = 0; i < spec.Orderings.Count; i++)
                {
                    QueryOrdering<TAggregateRoot> ordering = spec.Orderings[i];

                    if (i == 0)
                    {
                        orderedQuery = ordering.Direction == SortDirection.Ascending
                            ? query.OrderBy(ordering.Expression)
                            : query.OrderByDescending(ordering.Expression);
                    }
                    else
                    {
                        orderedQuery = ordering.Direction == SortDirection.Ascending
                            ? orderedQuery!.ThenBy(ordering.Expression)
                            : orderedQuery!.ThenByDescending(ordering.Expression);
                    }
                }

                query = orderedQuery ?? query;
            }

            // Apply paging
            if (spec.Page != null)
            {
                query = query.Skip(spec.Page.Skip).Take(spec.Page.Take);
            }

            return query;
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
