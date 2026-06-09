using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Odin.DDD.Repositories
{
    /// <inheritdoc />
    public abstract class EntityFrameworkIdentityRepositoryBase<TAggregateRoot, TId, TDbContext>
        : EntityFrameworkRepositoryBase<TAggregateRoot, TDbContext>
        where TAggregateRoot : class, IIdentityAggregateRoot<TId>
        where TId : struct, IEqualityOperators<TId, TId, bool>
        where TDbContext : DbContext, IUnitOfWork
    {
        /// <summary>
        /// EntityFrameworkIdentityRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        protected EntityFrameworkIdentityRepositoryBase(TDbContext dbContext)
            : base(dbContext)
        {
        }

        /// <summary>
        /// EntityFrameworkRepositoryBase constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dbSetName">The name of the DbSet property or EF named entity type.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EntityFrameworkIdentityRepositoryBase(TDbContext dbContext, string dbSetName)
            : base(dbContext,dbSetName)
        {
        }
        
        /// <summary>
        /// Returns a list of aggregate root IDs that match the given specification.
        /// </summary>
        /// <param name="specification"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected Task<IReadOnlyList<TId>> FetchIdsAsync(
            IQuerySpecification<TAggregateRoot> specification,
            CancellationToken ct = default)
        {
            return FetchManyAsync(specification, aggregateRoot => aggregateRoot.Id, ct);
        }
    }
}
