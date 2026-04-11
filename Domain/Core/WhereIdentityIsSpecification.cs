using System.Numerics;

namespace Odin.Domain;

/// <summary>
/// Allows querying of an aggregate root by Id.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TId"></typeparam>
public class WhereIdIsEqualTo<TAggregateRoot, TId> : AbstractIdentityQuerySpecification<TAggregateRoot, TId> 
    where TAggregateRoot : class, IIdentityAggregateRoot<TId> 
    where TId : struct, IEqualityOperators<TId, TId, bool>
{
    /// <summary>
    /// Specify the Id to retrieve.
    /// </summary>
    /// <param name="id"></param>
    public WhereIdIsEqualTo(TId id)
    {
        Criteria = entity => entity.Id.Equals(id);
    }
}