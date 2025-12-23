using Odin.DesignContracts;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Represents the outcome of an attempt to schedule a job with the background jobs engine.
    /// </summary>
    public sealed record JobDetails
    {
        /// <summary>
        /// Background job details
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="scheduledFor"></param>
        public JobDetails(string jobId, DateTimeOffset scheduledFor)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(jobId));
            JobId = jobId;
            ScheduledFor = scheduledFor;
        }
        
        /// <summary>
        /// Unique Job Id
        /// </summary>
        public string JobId { get;  private set;}
        
        /// <summary>
        /// When scheduled for
        /// </summary>
        public DateTimeOffset ScheduledFor { get; private set;  }
        
    }
}