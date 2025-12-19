using System.Linq.Expressions;
using Odin.System;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Now redundant interface to abstract Hangfire's out-of-process background job management (which used to be static). 
    /// </summary>
    [Obsolete("Rather use Hangfire and it's IBackgroundJobClient directly.")]
    public interface IBackgroundProcessor
    {
        /// <summary>
        /// Schedules a once-off job
        /// </summary>
        /// <param name="taskExpression"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ResultValue<JobDetails> ScheduleJob<T>(Expression<Func<T, Task>> taskExpression, DateTimeOffset enqueueAt);

        /// <summary>
        /// Schedules a once-off job
        /// </summary>
        /// <param name="taskExpression"></param>
        /// <param name="enqueueIn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ResultValue<JobDetails>  ScheduleJob<T>(Expression<Action<T>> taskExpression, TimeSpan enqueueIn);
        
        /// <summary>
        /// Schedules a once-off job
        /// </summary>
        /// <param name="taskExpression"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ResultValue<JobDetails> ScheduleJob<T>(Expression<Action<T>> taskExpression, DateTimeOffset enqueueAt);

        /// <summary>
        /// Schedules a once-off job
        /// </summary>
        /// <param name="taskExpression"></param>
        /// <param name="enqueueIn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ResultValue<JobDetails>  ScheduleJob<T>(Expression<Func<T, Task>> taskExpression, TimeSpan enqueueIn);

        /// <summary>
        /// Ensures a recurring job is registered.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="recurringJobName"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZoneInfo"></param>
        /// <param name="queueName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Result AddOrUpdateRecurringJob<T>(Expression<Action<T>> methodCall, string recurringJobName, string cronExpression, TimeZoneInfo timeZoneInfo, string queueName = "default");
        
        /// <summary>
        /// Ensures a recurring job is deleted, if it exists...
        /// </summary>
        /// <param name="recurringJobName"></param>
        /// <returns></returns>
        Result RemoveRecurringJob(string recurringJobName);

    }
}