using System.Linq.Expressions;
using Odin.System;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Fake provider for testing purposes. Use as an alternative to tedious mocking of Expressions...
    /// </summary>
    public sealed class NullBackgroundProcessor : IBackgroundProcessor
    {
        /// <summary>
        /// Default contructor
        /// </summary>
        public NullBackgroundProcessor()
        {
        }

        /// <summary>
        /// Does nothing and returns a successful outcome
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        {
            return ResultValue<JobDetails>.Success(new JobDetails("1", enqueueAt));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskExpression"></param>
        /// <param name="enqueueIn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>(Expression<Action<T>> taskExpression, TimeSpan enqueueIn)
        {
            return ScheduleJob(taskExpression, DateTimeOffset.Now.Add(enqueueIn));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskExpression"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public ResultValue<JobDetails> ScheduleJob<T>(Expression<Action<T>> taskExpression, DateTimeOffset enqueueAt)
        {
            return ResultValue<JobDetails>.Success(new JobDetails("1", enqueueAt));
        }

        /// <summary>
        /// Does nothing and returns a successful outcome
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueIn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>(Expression<Func<T, Task>> methodCall, TimeSpan enqueueIn)
        {
            return ScheduleJob(methodCall, DateTimeOffset.Now.Add(enqueueIn));
        }

        /// <summary>
        /// Acts according to how the Behaviour property has been set.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="jobName"></param>
        /// <param name="cronExpression"></param>
        /// <param name="timeZoneInfo"></param>
        /// <param name="queueName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="Exception"></exception>
        public Result AddOrUpdateRecurringJob<T>(Expression<Action<T>> methodCall, string jobName, string cronExpression, TimeZoneInfo timeZoneInfo, string queueName = "default")
        {
            return Result.Success();
        }

        /// <summary>
        /// RemoveIfExists
        /// </summary>
        /// <param name="jobName"></param>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="Exception"></exception>
        public Result RemoveRecurringJob(string jobName)
        {
            return Result.Success();
        }
    }
}