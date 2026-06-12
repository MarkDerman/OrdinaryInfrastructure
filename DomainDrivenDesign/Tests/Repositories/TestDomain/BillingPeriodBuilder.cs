namespace Tests.Odin.DDD.Repositories.TestDomain;

public class BillingPeriodBuilder
{
    private BillingEntity? _billingEntity;
    private DateOnly _periodStarting = new DateOnly(2026, 1, 1);
    private DateOnly _periodEnding = new DateOnly(2026, 1, 31);
    private BillingPeriodStage _stage = BillingPeriodStage.PeriodInProgress;
    private BillingPeriodBillingStatus _billingStatus = BillingPeriodBillingStatus.NotProcessed;

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


    public BillingPeriod Build()
    {
        BillingEntity billingEntity = _billingEntity ?? new BillingEntityBuilder().Build();
        BillingPeriod billingPeriod = new BillingPeriod(billingEntity, _periodStarting, _periodEnding, _stage)
        {
            BillingStatus = _billingStatus
        };

        return billingPeriod;
    }
}
