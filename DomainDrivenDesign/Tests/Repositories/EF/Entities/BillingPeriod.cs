using Odin.DDD;

namespace Tests.Odin.DDD.Repositories.EF.Entities
{
    /// <summary>
    ///    Testing entity
    /// </summary>
    public class BillingPeriod : IAggregateRoot
    {
        /// <summary>
        /// For EF
        /// </summary>
        protected BillingPeriod()
        {
        }

        /// <summary>
        /// Creates a new BillingPeriod
        /// </summary>
        /// <param name="billingEntity"></param>
        /// <param name="startInclusive"></param>
        /// <param name="endInclusive"></param>
        /// <param name="initialStage"></param>
        public BillingPeriod(BillingEntity billingEntity, DateOnly startInclusive,
            DateOnly endInclusive, short initialStage)
        {
            ArgumentNullException.ThrowIfNull(billingEntity);
            BillingEntity = billingEntity;
            BillingEntityId = billingEntity.Id;
            PeriodStarting = startInclusive;
            PeriodEnding = endInclusive;
            Stage = initialStage;
        }



        /// <summary>
        ///  Id
        /// </summary>
        public long Id { get; protected internal set; }

        /// <summary>
        ///     The Customer that this BillingPeriod is associated with
        /// </summary>
        public int BillingEntityId { get; protected set; }

        public virtual BillingEntity BillingEntity { get; protected set; } = null!;

        /// <summary>
        ///     The first day of the BillingPeriod
        /// </summary>
        public DateOnly PeriodStarting { get; protected set; }

        /// <summary>
        ///     The last day of the BillingPeriod
        /// </summary>
        public DateOnly PeriodEnding { get; protected set; }


        /// <summary>
        ///  The current Stage of processing
        /// </summary>
        public int Stage { get; set; }

        public BillingPeriodStatus Status { get; set; }


    }
    
    public enum BillingPeriodStatus : short
    {
        Failed = -1,
        NotProcessed = 0,
        Completed = 3,
        Processing = 4,
    }
}