namespace Tests.Odin.DDD.Repositories.TestDomain;

public abstract class BillingEntityBase : IBillingEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; protected set; }

    public string Name { get; set; } = null!;

    public BillingEntityStatus Status { get; set; } = BillingEntityStatus.Active;
}
