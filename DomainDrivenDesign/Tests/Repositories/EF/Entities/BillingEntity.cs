namespace Tests.Odin.DDD.Repositories.EF.Entities;

/// <summary>
/// Test entity 1
/// </summary>
public class BillingEntity : BillingEntityBase
{
    /// <summary>
    /// For EF
    /// </summary>
    protected BillingEntity()
    {}

    public BillingEntity(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string? BillingName { get; set; }

    public string? BillingAddress { get; set; }

    public string? VatNumber { get; set; }
}

public enum BillingEntityStatus : short
{
    NotActive = 0,
    Active = 1,
}
