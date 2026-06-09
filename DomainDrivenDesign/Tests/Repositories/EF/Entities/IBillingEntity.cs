using Odin.DDD;

namespace Tests.Odin.DDD.Repositories.EF.Entities;

public interface IBillingEntity : IIdentityAggregateRoot<int>
{
    string Name { get; }

    BillingEntityStatus Status { get; }
}
