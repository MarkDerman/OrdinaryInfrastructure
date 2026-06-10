using Odin.DDD;

namespace Tests.Odin.DDD.Repositories.TestDomain;

public interface IBillingEntity : IIdentityAggregateRoot<int>
{
    string Name { get; }

    BillingEntityStatus Status { get; }
}

public enum BillingEntityStatus : short
{
    NotActive = 0,
    Active = 1,
}
