namespace Tests.Odin.DDD.Repositories.TestDomain;

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


