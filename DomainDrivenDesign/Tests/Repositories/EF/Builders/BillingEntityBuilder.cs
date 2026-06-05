using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories.EF.Builders;

public class BillingEntityBuilder
{
    private string _name = "Billing Entity";
    private string? _billingName = "Billing Entity Ltd";
    private string? _billingAddress = "1 Test Street";
    private string? _vatNumber = "VAT-001";
    private BillingEntityStatus _status = BillingEntityStatus.Active;

    public BillingEntityBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public BillingEntityBuilder WithBillingName(string? billingName)
    {
        _billingName = billingName;
        return this;
    }

    public BillingEntityBuilder WithBillingAddress(string? billingAddress)
    {
        _billingAddress = billingAddress;
        return this;
    }

    public BillingEntityBuilder WithVatNumber(string? vatNumber)
    {
        _vatNumber = vatNumber;
        return this;
    }

    public BillingEntityBuilder WithStatus(BillingEntityStatus status)
    {
        _status = status;
        return this;
    }

    public BillingEntity Build()
    {
        return new BillingEntity(_name)
        {
            BillingName = _billingName,
            BillingAddress = _billingAddress,
            VatNumber = _vatNumber,
            Status = _status
        };
    }
}
