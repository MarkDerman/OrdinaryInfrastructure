using System.Linq.Expressions;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Odin.Domain.EntityFramework;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public abstract class AbstractIdentityRepository<TAggregateRoot, TId, TDbContext> 
    : AbstractRepository<TAggregateRoot, TDbContext>, IRepository<TAggregateRoot, TId>
    where TDbContext : DbContext, IUnitOfWork
    where TAggregateRoot : class, IIdentityAggregateRoot<TId>
    where TId : struct, IEqualityOperators<TId, TId, bool>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    protected AbstractIdentityRepository(TDbContext dbContext) : base(dbContext)
    {
    }
    
    /// <inheritdoc />
    public async Task<TAggregateRoot?> FetchByIdAsync(TId id, CancellationToken ct = default)
    {
        var query = new WhereIdIsEqualTo<TAggregateRoot, TId>(id);
        return await FetchAsync(query, ct);
    }

    /// <inheritdoc />
    public Task<TAggregateRoot?> FetchByIdAsync(TId id, 
        IReadOnlyList<Expression<Func<TAggregateRoot, object>>>? includes, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TAggregateRoot>> FetchManyByIdAsync(params ReadOnlySpan<TId> ids)
    {
        var idList = ids.ToArray();
        return FetchManyByIdAsyncInternal(idList);
    }

    private async Task<IReadOnlyList<TAggregateRoot>> FetchManyByIdAsyncInternal(TId[] idList)
    {
        return await DbSet.Where(x => idList.Contains(x.Id)).ToListAsync();
    }
}