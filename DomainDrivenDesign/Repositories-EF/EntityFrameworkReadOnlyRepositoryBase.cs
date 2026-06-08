using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Odin.DDD.Repositories
{
    /// <summary>
    /// Provides a base implementation for read-only repositories that use Entity Framework Core for data access.
    /// Exposes protected FetchSingleAsync and FetchManyAsync query endpoints for concrete repository methods.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The aggregate root type.</typeparam>
    /// <typeparam name="TDbContext">The database context that must contain a DBSet of <typeparamref name="TAggregateRoot"/>.</typeparam>
    public abstract class EntityFrameworkReadOnlyRepositoryBase<TAggregateRoot, TDbContext>
        : IReadOnlyRepository<TAggregateRoot>, IDisposable
        where TDbContext : DbContext
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// The context for database operations.
        /// </summary>
        protected readonly TDbContext DbContext;

        /// <summary>
        /// The DbSet for the aggregate root type, TAggregateRoot.
        /// </summary>
        protected readonly DbSet<TAggregateRoot> DbSet;

        /// <summary>
        /// EntityFrameworkReadOnlyRepositoryBase default constructor, for use when an exact
        /// DbSet<TAggregateRoot> is available in the DbContext.
        /// </summary>
        /// <param name="dbContext">The Entity Framework database context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkReadOnlyRepositoryBase(TDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            DbContext = dbContext;
            DbSet = DbContext.Set<TAggregateRoot>();
        }
        
        /// <summary>
        /// Used when for TAggregateRoot is a base type, or interface declaration of an actual implementation type
        /// in the database context.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbSetName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkReadOnlyRepositoryBase(TDbContext dbContext, string dbSetName)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            DbContext = dbContext;
            DbSet = DbContext.Set<TAggregateRoot>(dbSetName);
        }

        /// <summary>
        /// Gets a value indicating whether queries should be created without EF change tracking.
        /// </summary>
        protected virtual bool UseNoTrackingQueries
        {
            get { return true; }
        }

        /// <summary>
        /// Queries the repository based on the query specification.
        /// </summary>
        /// <param name="fetchManySpec">The query specification.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The aggregate roots that match the query specification.</returns>
        protected async Task<IReadOnlyList<TAggregateRoot>> FetchManyAsync(
            IQuerySpecification<TAggregateRoot> fetchManySpec, CancellationToken ct = default)
        {
            return await ApplySpecification(fetchManySpec).ToListAsync(ct);
        }

        /// <summary>
        /// Queries the repository based on the query specification, projecting TAggregateRoot
        /// to TProjection.
        /// </summary>
        /// <param name="specification">The query specification.</param>
        /// <param name="projection">The projection to apply.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <typeparam name="TProjection">The projection result type.</typeparam>
        /// <returns>The projected results that match the query specification.</returns>
        protected async Task<IReadOnlyList<TProjection>> FetchManyAsync<TProjection>(
            IQuerySpecification<TAggregateRoot> specification,
            Expression<Func<TAggregateRoot, TProjection>> projection,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return await ApplySpecification(specification)
                .Select(projection)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Queries the repository based on the projection query specification, projecting TAggregateRoot
        /// to TProjection.
        /// </summary>
        /// <param name="specification">The projection query specification.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <typeparam name="TProjection">The projection result type.</typeparam>
        /// <returns>The projected results that match the query specification.</returns>
        protected async Task<IReadOnlyList<TProjection>> FetchManyAsync<TProjection>(
            IProjectedQuerySpecification<TAggregateRoot, TProjection> specification,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(specification);

            return await FetchManyAsync(specification, specification.Projection, ct);
        }

        /// <summary>
        /// Queries for a single entity from the query spec, returning null if
        /// no result returned from the repository.
        /// Throws an exception if multiple entities are found.
        /// </summary>
        /// <param name="fetchOneSpec">The single entity query specification.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The aggregate root that matches the query specification, or null if none exists.</returns>
        protected async Task<TAggregateRoot?> FetchSingleAsync(
            ISingleEntityQuerySpecification<TAggregateRoot> fetchOneSpec, CancellationToken ct = default)
        {
            List<TAggregateRoot> results = await AsSingleResultQueryable(fetchOneSpec).ToListAsync(ct);
            if (results.Count > 1)
            {
                throw new InvalidOperationException($"Expected single result, but found {results.Count}.");
            }
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Applies the query criteria and eager-loading includes, if any.
        /// </summary>
        /// <param name="singleEntityQuerySpecification">The single entity query specification.</param>
        /// <returns>The resulting queryable.</returns>
        protected IQueryable<TAggregateRoot> AsSingleResultQueryable(
            ISingleEntityQuerySpecification<TAggregateRoot> singleEntityQuerySpecification)
        {
            ArgumentNullException.ThrowIfNull(singleEntityQuerySpecification);
            IQueryable<TAggregateRoot> query = CreateQuery()
                .Where(singleEntityQuerySpecification.Criteria);

            if (singleEntityQuerySpecification.Includes != null)
            {
                foreach (Expression<Func<TAggregateRoot, object>> include in singleEntityQuerySpecification.Includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }

        /// <summary>
        /// Applies the query specification to the EF DbSet for TAggregateRoot
        /// and returns the resulting IQueryable of TAggregateRoot.
        /// </summary>
        /// <param name="querySpecification">The query specification.</param>
        /// <returns>The resulting queryable.</returns>
        protected IQueryable<TAggregateRoot> ApplySpecification(IQuerySpecification<TAggregateRoot> querySpecification)
        {
            ArgumentNullException.ThrowIfNull(querySpecification);
            IQueryable<TAggregateRoot> query = CreateQuery();

            if (querySpecification.Criteria != null)
            {
                query = query.Where(querySpecification.Criteria);
            }

            if (querySpecification.Includes != null)
            {
                foreach (Expression<Func<TAggregateRoot, object>> include in querySpecification.Includes)
                {
                    query = query.Include(include);
                }
            }

            if (querySpecification.Orderings != null)
            {
                IOrderedQueryable<TAggregateRoot>? orderedQuery = null;

                for (int i = 0; i < querySpecification.Orderings.Count; i++)
                {
                    QueryOrdering<TAggregateRoot> ordering = querySpecification.Orderings[i];

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

            if (querySpecification.Page != null)
            {
                query = query.Skip(querySpecification.Page.Skip).Take(querySpecification.Page.Take);
            }

            return query;
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

        private IQueryable<TAggregateRoot> CreateQuery()
        {
            IQueryable<TAggregateRoot> query = DbSet.AsQueryable();

            if (UseNoTrackingQueries)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}
