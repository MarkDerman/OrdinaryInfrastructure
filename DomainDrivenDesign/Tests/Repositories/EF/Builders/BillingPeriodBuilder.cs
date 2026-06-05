using Tests.Odin.DDD.Repositories.EF.Entities;

namespace Tests.Odin.DDD.Repositories.EF.Builders;

public class BillingPeriodBuilder
{
    private BillingEntity? _billingEntity;
    private DateOnly _periodStarting = new DateOnly(2026, 1, 1);
    private DateOnly _periodEnding = new DateOnly(2026, 1, 31);
    private BillingPeriodStage _stage = BillingPeriodStage.PeriodInProgress;
    private BillingPeriodBillingStatus _billingStatus = BillingPeriodBillingStatus.NotProcessed;
    private readonly List<BillingPeriodPropertyBuilder> _propertyBuilders = [];
    private readonly List<BillingPeriodTaskBuilder> _taskBuilders = [];

    public BillingPeriodBuilder ForBillingEntity(BillingEntity billingEntity)
    {
        _billingEntity = billingEntity;
        return this;
    }

    public BillingPeriodBuilder WithPeriod(DateOnly periodStarting, DateOnly periodEnding)
    {
        _periodStarting = periodStarting;
        _periodEnding = periodEnding;
        return this;
    }

    public BillingPeriodBuilder WithStage(BillingPeriodStage stage)
    {
        _stage = stage;
        return this;
    }

    public BillingPeriodBuilder WithBillingStatus(BillingPeriodBillingStatus billingStatus)
    {
        _billingStatus = billingStatus;
        return this;
    }

    public BillingPeriodBuilder WithProperty(BillingPeriodPropertyBuilder propertyBuilder)
    {
        _propertyBuilders.Add(propertyBuilder);
        return this;
    }

    public BillingPeriodBuilder WithProperty(string propertyName, DataType dataType, string? dataValue)
    {
        return WithProperty(new BillingPeriodPropertyBuilder()
            .WithName(propertyName)
            .WithDataType(dataType)
            .WithDataValue(dataValue));
    }

    public BillingPeriodBuilder WithTask(BillingPeriodTaskBuilder taskBuilder)
    {
        _taskBuilders.Add(taskBuilder);
        return this;
    }

    public BillingPeriodBuilder WithTask(BillingPeriodTaskType taskType, BillingPeriodStage stage)
    {
        return WithTask(new BillingPeriodTaskBuilder()
            .WithTaskType(taskType)
            .WithStage(stage));
    }

    public BillingPeriod Build()
    {
        BillingEntity billingEntity = _billingEntity ?? new BillingEntityBuilder().Build();
        BillingPeriod billingPeriod = new BillingPeriod(billingEntity, _periodStarting, _periodEnding, _stage)
        {
            BillingStatus = _billingStatus
        };

        foreach (BillingPeriodPropertyBuilder propertyBuilder in _propertyBuilders)
        {
            billingPeriod.Properties.Add(propertyBuilder.Build(billingPeriod));
        }

        foreach (BillingPeriodTaskBuilder taskBuilder in _taskBuilders)
        {
            billingPeriod.Tasks.Add(taskBuilder.Build(billingPeriod));
        }

        return billingPeriod;
    }
}
