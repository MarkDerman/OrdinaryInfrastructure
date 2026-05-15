using System.Linq.Expressions;
using System.Numerics;

namespace Odin.DDD.Repositories;

/// <inheritdoc />
public abstract class IdentityQuerySpecification<TAggregateRoot, TId> 
    : SingleEntityQuerySpecification<TAggregateRoot> 
    where TAggregateRoot : class, IIdentityAggregateRoot<TId>
    where TId : struct, IEqualityOperators<TId, TId, bool>
{
    /// <summary>
    /// Default constructor for AbstractQuerySpecification
    /// </summary>
    /// <param name="criteria"></param>
    protected IdentityQuerySpecification(Expression<Func<TAggregateRoot, bool>>? criteria) : base(criteria)
    {
    }
}