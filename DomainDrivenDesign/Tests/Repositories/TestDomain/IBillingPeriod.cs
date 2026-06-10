using Odin.DDD;

namespace Tests.Odin.DDD.Repositories.TestDomain;

public interface IBillingPeriod<out TBillingEntity> : IAggregateRoot
    where TBillingEntity : class, IBillingEntity
{
    /// <summary>
    ///  Id
    /// </summary>
    long Id { get; }

    /// <summary>
    ///     The Customer that this BillingPeriod is associated with
    /// </summary>
    int BillingEntityId { get; }

    TBillingEntity BillingEntity { get; }

    /// <summary>
    ///     The first day of the BillingPeriod
    /// </summary>
    DateOnly PeriodStarting { get; }

    /// <summary>
    ///     The last day of the BillingPeriod
    /// </summary>
    DateOnly PeriodEnding { get; }

    /// <summary>
    ///  The current Stage of processing
    /// </summary>
    BillingPeriodStage Stage { get; set; }

    BillingPeriodBillingStatus BillingStatus { get; set; }
    ICollection<BillingPeriodProperty> Properties { get; }
    IList<BillingPeriodTask> Tasks { get; }
}