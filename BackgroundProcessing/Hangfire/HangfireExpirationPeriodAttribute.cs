using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Causes Hangfire to keep completed and deleted jobs on the dashboard for the specified period.
    /// Note that the Hangfire default is 1 day.
    /// </summary>
    public sealed class HangfireExpirationPeriodAttribute : JobFilterAttribute, IApplyStateFilter
    {
        /// <summary>
        /// ExpirationPeriod
        /// </summary>
        public TimeSpan ExpirationPeriod { get; }

        /// <summary>
        /// Takes a TimeSpan for the expiration period
        /// </summary>
        /// <param name="expirationPeriodForCompletedJobs"></param>
        public HangfireExpirationPeriodAttribute(TimeSpan expirationPeriodForCompletedJobs)
        {
            ExpirationPeriod = expirationPeriodForCompletedJobs;
        }

        /// <summary>
        /// Sets the job expiration timeout
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transaction"></param>
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = ExpirationPeriod;
        }

        /// <summary>
        /// Not needed?
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transaction"></param>
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            // Not sure what to do here, if anything?
        }
    }
}