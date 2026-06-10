namespace Tests.Odin.DDD.Repositories.EF.Entities
{
    /// <summary>
    ///    Testing entity
    /// </summary>
    public class BillingPeriod  : IBillingPeriod<BillingEntity>
    
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
            DateOnly endInclusive, BillingPeriodStage initialStage)
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
        public BillingPeriodStage Stage { get; set; }

        public BillingPeriodBillingStatus BillingStatus { get; set; }

        public IReadOnlyCollection<BillingPeriodProperty> Properties { get; protected set; } = new List<BillingPeriodProperty>();

        public IReadOnlyList<BillingPeriodTask> Tasks { get; protected set; } = new List<BillingPeriodTask>();
        

    }
    
    public enum BillingPeriodBillingStatus : short
    {
        Failed = -1,
        NotProcessed = 0,
        Completed = 3,
        Processing = 4,
    }

    public enum BillingPeriodStage : short
    {
        PeriodInProgress = 0,
        RecordBillables = 20,
        PostCustomerInvoice = 40,
        PostVendorBill = 50,
        SettleVendorBill = 60,
        Done = 100,
    }

    public enum DataType : short
    {
        String = 0,
        Guid = 1,
        DateTime = 2,
        DateTimeOffset = 3,
        Int64 = 4,
    }

    public enum BillingPeriodTaskStatus : short
    {
        Failed = -2,
        FailedToBeRetried = -1,
        New = 0,
        Succeeded = 2,
    }

    public enum BillingPeriodTaskType : short
    {
        RefreshSalesFeesAndCommissionTotalsByBillingCode = 20,
        CreateEndOfBillingPeriodSageARCustomerInvoice = 40,
        PostEndOfBillingPeriodSageARCustomerInvoice = 41,
        CreateEndOfBillingPeriodSageAPVendorInvoice = 50,
        PostEndOfBillingPeriodSageAPVendorInvoice = 51,
        CreateEndOfBillingPeriodSageARCustomerCreditNote = 60,
        PostEndOfBillingPeriodSageARCustomerCreditNote = 61,
        CreateEndOfBillingPeriodSageARReceiptAndAdjustment = 62,
        PostEndOfBillingPeriodSageARReceiptAndAdjustment = 63,
        CreateEndOfBillingPeriodSageAPVendorCreditNote = 64,
        PostEndOfBillingPeriodSageAPVendorCreditNote = 65,
        CreateEndOfBillingPeriodSageAPPaymentAndAdjustment = 66,
        PostEndOfBillingPeriodSageAPPaymentAndAdjustment = 67,
    }
}
