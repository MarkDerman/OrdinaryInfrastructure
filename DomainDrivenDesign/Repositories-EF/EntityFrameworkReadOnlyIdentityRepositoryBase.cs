using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Odin.DDD.Repositories
{
    /// <summary>
    /// Provides a base implementation for read-only repositories of 'Identity' AggregateRoot entities
    /// that uses Entity Framework Core for data access.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The aggregate root type.</typeparam>
    /// <typeparam name="TId">The aggregate root identity type.</typeparam>
    /// <typeparam name="TDbContext">The database context that must contain a DBSet of <typeparamref name="TAggregateRoot"/>.</typeparam>
    public abstract class EntityFrameworkReadOnlyIdentityRepositoryBase<TAggregateRoot, TId, TDbContext>
        : EntityFrameworkReadOnlyRepositoryBase<TAggregateRoot, TDbContext>
        where TAggregateRoot : class, IIdentityAggregateRoot<TId>
        where TId : struct, IEqualityOperators<TId, TId, bool>
        where TDbContext : DbContext
    {
        /// <summary>
        /// EntityFrameworkReadOnlyIdentityRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext">The Entity Framework database context.</param>
        protected EntityFrameworkReadOnlyIdentityRepositoryBase(TDbContext dbContext)
            : base(dbContext)
        {
        }
        
        /// <summary>
        /// EntityFrameworkRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbSetName">The name of the DbSet property or EF named entity type.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkReadOnlyIdentityRepositoryBase(TDbContext dbContext, string dbSetName)
            : base(dbContext,dbSetName)
        {
        }

        /// <summary>
        /// EntityFrameworkRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbSet">The DbSet property</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkReadOnlyIdentityRepositoryBase(TDbContext dbContext, DbSet<TAggregateRoot> dbSet)
            :base( dbContext, dbSet)
        {
        }
        
        /// <summary>
        /// Returns a list of aggregate root IDs that match the given specification.
        /// </summary>
        /// <param name="specification">The query specification.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The aggregate root IDs that match the query specification.</returns>
        protected Task<IReadOnlyList<TId>> FetchIdsAsync(
            IQuerySpecification<TAggregateRoot> specification,
            CancellationToken ct = default)
        {
            return FetchManyAsync(specification, aggregateRoot => aggregateRoot.Id, ct);
        }
    }
}
