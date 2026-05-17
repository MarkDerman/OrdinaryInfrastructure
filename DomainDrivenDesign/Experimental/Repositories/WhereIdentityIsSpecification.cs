// using System.Numerics;
//
// namespace Odin.DDD;
//
// /// <summary>
// /// Allows querying of an aggregate root by Id.
// /// </summary>
// /// <typeparam name="TIdentityAggregateRoot"></typeparam>
// /// <typeparam name="TId"></typeparam>
// public class WhereIdIsEqualTo<TIdentityAggregateRoot, TId> : SingleEntityQuerySpecification<TIdentityAggregateRoot, TId> 
//     where TIdentityAggregateRoot : class, IIdentityAggregateRoot<TId> 
//     where TId : struct, IEqualityOperators<TId, TId, bool>
// {
//     /// <summary>
//     /// Specify the Id to retrieve.
//     /// </summary>
//     /// <param name="id"></param>
//     public WhereIdIsEqualTo(TId id) : base(entity => entity.Id.Equals(id))
//     {
//     }
// }