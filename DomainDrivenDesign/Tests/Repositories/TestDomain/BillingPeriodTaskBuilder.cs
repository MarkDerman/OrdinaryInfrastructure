namespace Tests.Odin.DDD.Repositories.TestDomain;

public class BillingPeriodTaskBuilder
{
    private BillingPeriodTaskType _taskType = BillingPeriodTaskType.RefreshSalesFeesAndCommissionTotalsByBillingCode;
    private BillingPeriodTaskStatus _status = BillingPeriodTaskStatus.New;
    private string _dependsOn = "[]";
    private DateTimeOffset? _lastAttemptedAt;
    private DateTimeOffset? _waitUntil;
    private string? _data;
    private BillingPeriodStage _stage = BillingPeriodStage.RecordBillables;

    public BillingPeriodTaskBuilder WithTaskType(BillingPeriodTaskType taskType)
    {
        _taskType = taskType;
        return this;
    }

    public BillingPeriodTaskBuilder WithStatus(BillingPeriodTaskStatus status)
    {
        _status = status;
        return this;
    }

    public BillingPeriodTaskBuilder WithDependsOn(string dependsOn)
    {
        _dependsOn = dependsOn;
        return this;
    }

    public BillingPeriodTaskBuilder WithLastAttemptedAt(DateTimeOffset? lastAttemptedAt)
    {
        _lastAttemptedAt = lastAttemptedAt;
        return this;
    }

    public BillingPeriodTaskBuilder WithWaitUntil(DateTimeOffset? waitUntil)
    {
        _waitUntil = waitUntil;
        return this;
    }

    public BillingPeriodTaskBuilder WithData(string? data)
    {
        _data = data;
        return this;
    }

    public BillingPeriodTaskBuilder WithStage(BillingPeriodStage stage)
    {
        _stage = stage;
        return this;
    }

    public BillingPeriodTask Build(BillingPeriod billingPeriod)
    {
        return new BillingPeriodTask(billingPeriod, _taskType, _stage)
        {
            Status = _status,
            DependsOn = _dependsOn,
            LastAttemptedAt = _lastAttemptedAt,
            WaitUntil = _waitUntil,
            Data = _data
        };
    }
}
