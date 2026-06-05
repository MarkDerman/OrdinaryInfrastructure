namespace Tests.Odin.DDD.Repositories.EF.Entities;

public class BillingPeriodProperty
{
    protected BillingPeriodProperty()
    {
    }

    public BillingPeriodProperty(BillingPeriod billingPeriod, string propertyName, DataType dataType, string? dataValue)
    {
        ArgumentNullException.ThrowIfNull(billingPeriod);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        BillingPeriod = billingPeriod;
        BillingPeriodId = billingPeriod.Id;
        PropertyName = propertyName;
        DataType = dataType;
        DataValue = dataValue;
    }

    public long Id { get; protected internal set; }

    public long BillingPeriodId { get; protected set; }

    public virtual BillingPeriod BillingPeriod { get; protected set; } = null!;

    public string PropertyName { get; protected set; } = null!;

    public DataType DataType { get; protected set; }

    public string? DataValue { get; protected set; }
}
