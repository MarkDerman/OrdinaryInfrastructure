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

        // private static async Task<IPagedList<T>> ToPagedListAsync<T>(
        //     IQueryable<T> queryable,
        //     int pageNo,
        //     int pageSize,
        //     CancellationToken cancellationToken = default)
        // {
        //     var count = await queryable.CountAsync(cancellationToken);
        //     var skip = ((pageNo - 1) * pageSize);
        //
        //     var results = await queryable
        //         .Skip(skip)
        //         .Take(pageSize)
        //         .ToListAsync(cancellationToken);
        //     return new PagedList<T>(count, pageNo, pageSize, results);
        // }
        
        
        // public virtual async Task<TAggregateRoot?> FindAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression).SingleOrDefaultAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<TAggregateRoot?> FindAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression, queryOptions).SingleOrDefaultAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<TAggregateRoot?> FindAsync(
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(queryOptions).SingleOrDefaultAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<List<TAggregateRoot>> FindAllAsync(CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression: null).ToListAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<List<TAggregateRoot>> FindAllAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression).ToListAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<List<TAggregateRoot>> FindAllAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression, queryOptions).ToListAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<IPagedList<TAggregateRoot>> FindAllAsync(
        //     int pageNo,
        //     int pageSize,
        //     CancellationToken cancellationToken = default)
        // {
        //     var query = QueryInternal(filterExpression: null);
        //     return await ToPagedListAsync<TAggregateRoot>(
        //         query,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public virtual async Task<IPagedList<TAggregateRoot>> FindAllAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     int pageNo,
        //     int pageSize,
        //     CancellationToken cancellationToken = default)
        // {
        //     var query = QueryInternal(filterExpression);
        //     return await ToPagedListAsync<TAggregateRoot>(
        //         query,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public virtual async Task<IPagedList<TAggregateRoot>> FindAllAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     int pageNo,
        //     int pageSize,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var query = QueryInternal(filterExpression, queryOptions);
        //     return await ToPagedListAsync<TAggregateRoot>(
        //         query,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public virtual async Task<List<TAggregateRoot>> FindAllAsync(
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(queryOptions).ToListAsync<TAggregateRoot>(cancellationToken);
        // }
        //
        // public virtual async Task<IPagedList<TAggregateRoot>> FindAllAsync(
        //     int pageNo,
        //     int pageSize,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var query = QueryInternal(queryOptions);
        //     return await ToPagedListAsync<TAggregateRoot>(
        //         query,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public virtual async Task<int> CountAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression).CountAsync(cancellationToken);
        // }
        //
        // public virtual async Task<int> CountAsync(
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>>? queryOptions = default,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(queryOptions).CountAsync(cancellationToken);
        // }
        //
        // public virtual async Task<bool> AnyAsync(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(filterExpression).AnyAsync(cancellationToken);
        // }
        //
        // public virtual async Task<bool> AnyAsync(
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>>? queryOptions = default,
        //     CancellationToken cancellationToken = default)
        // {
        //     return await QueryInternal(queryOptions).AnyAsync(cancellationToken);
        // }
        //
        // public async Task<List<TProjection>> FindAllProjectToAsync<TProjection>(CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression: null);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.ToListAsync(cancellationToken);
        // }
        //
        // public async Task<List<TProjection>> FindAllProjectToAsync<TProjection>(
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(queryOptions);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.ToListAsync(cancellationToken);
        // }
        //
        // public async Task<List<TProjection>> FindAllProjectToAsync<TProjection>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.ToListAsync(cancellationToken);
        // }
        //
        // public async Task<List<TProjection>> FindAllProjectToAsync<TProjection>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression, queryOptions);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.ToListAsync(cancellationToken);
        // }
        //
        // public async Task<IPagedList<TProjection>> FindAllProjectToAsync<TProjection>(
        //     int pageNo,
        //     int pageSize,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression: null);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await ToPagedListAsync(
        //         projection,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public async Task<IPagedList<TProjection>> FindAllProjectToAsync<TProjection>(
        //     int pageNo,
        //     int pageSize,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(queryOptions);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await ToPagedListAsync(
        //         projection,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public async Task<IPagedList<TProjection>> FindAllProjectToAsync<TProjection>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     int pageNo,
        //     int pageSize,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await ToPagedListAsync(
        //         projection,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public async Task<IPagedList<TProjection>> FindAllProjectToAsync<TProjection>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     int pageNo,
        //     int pageSize,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression, queryOptions);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await ToPagedListAsync(
        //         projection,
        //         pageNo,
        //         pageSize,
        //         cancellationToken);
        // }
        //
        // public async Task<TProjection?> FindProjectToAsync<TProjection>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.FirstOrDefaultAsync(cancellationToken);
        // }
        //
        // public async Task<TProjection?> FindProjectToAsync<TProjection>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(filterExpression, queryOptions);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.FirstOrDefaultAsync(cancellationToken);
        // }
        //
        // public async Task<TProjection?> FindProjectToAsync<TProjection>(
        //     Func<IQueryable<TPersistence>, IQueryable<TPersistence>> queryOptions,
        //     CancellationToken cancellationToken = default)
        // {
        //     var queryable = QueryInternal(queryOptions);
        //     var projection = queryable.ProjectTo<TProjection>(_mapper.ConfigurationProvider);
        //     return await projection.FirstOrDefaultAsync(cancellationToken);
        // }
        //
        // protected virtual IQueryable<TPersistence> QueryInternal(Expression<Func<TPersistence, bool>>? filterExpression)
        // {
        //     var queryable = CreateQuery();
        //     if (filterExpression != null)
        //     {
        //         queryable = queryable.Where(filterExpression);
        //     }
        //     return queryable;
        // }
        //
        // protected virtual IQueryable<TResult> QueryInternal<TResult>(
        //     Expression<Func<TPersistence, bool>> filterExpression,
        //     Func<IQueryable<TPersistence>, IQueryable<TResult>> queryOptions)
        // {
        //     var queryable = CreateQuery();
        //     queryable = queryable.Where(filterExpression);
        //     var result = queryOptions(queryable);
        //     return result;
        // }
        //
        // protected virtual IQueryable<TAggregateRoot> QueryInternal(Func<IQueryable<TAggregateRoot>, IQueryable<TAggregateRoot>>? queryOptions)
        // {
        //     var queryable = CreateQuery();
        //     if (queryOptions != null)
        //     {
        //         queryable = queryOptions(queryable);
        //     }
        //     return queryable;
        // }
        //
        // protected virtual IQueryable<TAggregateRoot> CreateQuery()
        // {
        //     return DbSet;
        // }


    }
}