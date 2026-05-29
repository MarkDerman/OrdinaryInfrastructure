
namespace Tests.Odin.DDD.Repositories.EF.Entities;

/// <summary>
/// Test
/// </summary>
public class BillingEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; protected set; }
    
    /// <summary>
    /// For EF
    /// </summary>
    protected BillingEntity()
    {}

    public string Name { get; set; } = null!;
    
    public string? BillingName { get; set; }

    public string? BillingAddress { get; set; }

    public string? VatNumber { get; set; }

}
