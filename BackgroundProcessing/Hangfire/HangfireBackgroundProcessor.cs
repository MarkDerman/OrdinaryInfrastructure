using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Odin.DesignContracts;
using Odin.Logging;
using Odin.System;


namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Hangfire based IJobManager
    /// </summary>
    public sealed class HangfireBackgroundProcessor : IBackgroundProcessor
    {
        private readonly ILoggerWrapper<HangfireBackgroundProcessor> _logger;
        private readonly IRecurringJobManagerV2 _recurringJobManager;
        private readonly IBackgroundJobClient _jobClient;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="recurringJobManager"></param>
        /// <param name="jobClient"></param>
        /// <param name="logger"></param>
        public HangfireBackgroundProcessor(IRecurringJobManagerV2 recurringJobManager, IBackgroundJobClient jobClient, ILoggerWrapper<HangfireBackgroundProcessor> logger)
        {
            Contract.RequiresNotNull(recurringJobManager);
            Contract.RequiresNotNull(jobClient);
            Contract.RequiresNotNull(logger);
            _recurringJobManager = recurringJobManager;
            _jobClient = jobClient;
            _logger = logger;
            
        }

        /// <summary>
        /// Schedules a once-off job in Hangfire
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>([NotNull, InstantHandle] Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        {
            try
            {
                string jobId = _jobClient.Schedule<T>(methodCall, enqueueAt);
                return ResultValue<JobDetails>.Succeed(new JobDetails(jobId, enqueueAt));
            }
            catch (Exception err)
            {
                string message = $"Exception scheduling {methodCall.Name} for {enqueueAt}. {err.Message}";
                _logger.LogError($"{nameof(ScheduleJob)}: {message}", err);
                return ResultValue<JobDetails>.Failure(message);
            }
        }
        
        /// <summary>
        /// Schedules a once-off job in Hangfire
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>([NotNull, InstantHandle] Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        {
            try
            {
                string jobId = _jobClient.Schedule<T>(methodCall, enqueueAt);
                return ResultValue<JobDetails>.Succeed(new JobDetails(jobId, enqueueAt));
            }
            catch (Exception err)
            {
                string message = $"Exception scheduling {methodCall.Name} for {enqueueAt}. {err.Message}";
                _logger.LogError($"{nameof(ScheduleJob)}: {message}", err);
                return ResultValue<JobDetails>.Failure(message);
            }
        }

        /// <summary>
        /// Ensures a recurring job is registered in Hangfire.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="recurringJobId"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZoneInfo"></param>
        /// <param name="queueName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Result AddOrUpdateRecurringJob<T>(
            [NotNull, InstantHandle] Expression<Action<T>> methodCall,
            string recurringJobId, string cronExpression, TimeZoneInfo timeZoneInfo, string queueName = "default")
        {
            try
            {
                _recurringJobManager.AddOrUpdate<T>(recurringJobId, queueName, methodCall, cronExpression, new RecurringJobOptions(){ TimeZone = timeZoneInfo});
                return Result.Success();
            }
            catch (Exception err)
            {
                string message = $"Error scheduling recurring job {recurringJobId}. {err.Message}";
                _logger.LogError($"{nameof(AddOrUpdateRecurringJob)}: {message}", err);
                return Result.Failure(message);
            }
        }

        /// <summary>
        /// Removes a job if it exists
        /// </summary>
        /// <param name="jobName"></param>
        public Result RemoveRecurringJob(string jobName)
        {
            try
            {
                _recurringJobManager.RemoveIfExists(jobName);
                return Result.Success();
            }
            catch (Exception err)
            {
                string message = $"Error removing recurring job {jobName}. {err.Message}";
                _logger.LogError($"{nameof(RemoveRecurringJob)}: {message}", err);
                return Result.Failure(message);
            }
        }


        /// <summary>
        /// Schedules a once-off job in Hangfire
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueIn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>([NotNull, InstantHandle] Expression<Func<T, Task>> methodCall, TimeSpan enqueueIn)
        {
            return ScheduleJob(methodCall, DateTimeOffset.Now.Add(enqueueIn));
        }
        
        /// <summary>
        /// Schedules a once-off job in Hangfire
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueIn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>([NotNull, InstantHandle] Expression<Action<T>> methodCall, TimeSpan enqueueIn)
        {
            return ScheduleJob(methodCall, DateTimeOffset.Now.Add(enqueueIn));
        }
    }
}