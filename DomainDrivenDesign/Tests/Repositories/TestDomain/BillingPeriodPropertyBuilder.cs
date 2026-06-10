namespace Tests.Odin.DDD.Repositories.TestDomain;

public class BillingPeriodPropertyBuilder
{
    private string _propertyName = "CorrelationId";
    private DataType _dataType = DataType.String;
    private string? _dataValue = "test-correlation-id";

    public BillingPeriodPropertyBuilder WithName(string propertyName)
    {
        _propertyName = propertyName;
        return this;
    }

    public BillingPeriodPropertyBuilder WithDataType(DataType dataType)
    {
        _dataType = dataType;
        return this;
    }

    public BillingPeriodPropertyBuilder WithDataValue(string? dataValue)
    {
        _dataValue = dataValue;
        return this;
    }

    public BillingPeriodProperty Build(BillingPeriod billingPeriod)
    {
        return new BillingPeriodProperty(billingPeriod, _propertyName, _dataType, _dataValue);
    }
}
