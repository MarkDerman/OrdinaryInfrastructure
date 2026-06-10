namespace Tests.Odin.DDD.Repositories.TestDomain;

public class BillingPeriodTask
{
    protected BillingPeriodTask()
    {
    }

    public BillingPeriodTask(
        BillingPeriod billingPeriod,
        BillingPeriodTaskType taskType,
        BillingPeriodStage stage)
    {
        ArgumentNullException.ThrowIfNull(billingPeriod);

        BillingPeriod = billingPeriod;
        BillingPeriodId = billingPeriod.Id;
        TaskType = taskType;
        Stage = stage;
        Status = BillingPeriodTaskStatus.New;
        DependsOn = "[]";
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public long Id { get; protected internal set; }

    public long BillingPeriodId { get; protected set; }

    public virtual BillingPeriod BillingPeriod { get; protected set; } = null!;

    public BillingPeriodTaskType TaskType { get; protected set; }

    public BillingPeriodTaskStatus Status { get; set; }

    public string DependsOn { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset? LastAttemptedAt { get; set; }

    public DateTimeOffset? WaitUntil { get; set; }

    public string? Data { get; set; }

    public BillingPeriodStage Stage { get; set; }
}
