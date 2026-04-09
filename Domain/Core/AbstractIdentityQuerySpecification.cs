using System.Linq.Expressions;
using System.Numerics;

namespace Odin.Domain;

/// <inheritdoc />
public abstract class AbstractIdentityQuerySpecification<TAggregateRoot, TId> 
    : AbstractQuerySpecification<TAggregateRoot> 
    where TAggregateRoot : class, IIdentityAggregateRoot<TId>
    where TId : struct, IEqualityOperators<TId, TId, bool>
{
    /// <summary>
    /// Default constructor for AbstractQuerySpecification
    /// </summary>
    /// <param name="criteria"></param>
    protected AbstractIdentityQuerySpecification(Expression<Func<TAggregateRoot, bool>>? criteria) : base(criteria)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    protected AbstractIdentityQuerySpecification()
    {
    }
}