using System.Linq.Expressions;
using Odin.System;

namespace Odin.BackgroundProcessing
{
    /// <summary>
    /// Fake provider for testing purposes. Use as an alternative to tedious mocking of Expressions...
    /// </summary>
    public sealed class FakeBackgroundProcessor : IBackgroundProcessor
    {
        /// <summary>
        /// Default contructor
        /// </summary>
        /// <param name="behaviour"></param>
        public FakeBackgroundProcessor(
            FakeBackgroundJobProviderBehaviour behaviour =
                FakeBackgroundJobProviderBehaviour.ReturnSuccessfulOutcome)
        {
            Behaviour = behaviour;
        }
        
        /// <summary>
        /// Behaviour of fake provider
        /// </summary>
        public FakeBackgroundJobProviderBehaviour Behaviour { get; set; }
        
        /// <summary>
        /// Does nothing and returns a successful outcome
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ResultValue<JobDetails> ScheduleJob<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        {
            switch (Behaviour)
            {
                case FakeBackgroundJobProviderBehaviour.ReturnSuccessfulOutcome:
                    return ResultValue<JobDetails>.Succeed(new JobDetails("1", enqueueAt));
                case FakeBackgroundJobProviderBehaviour.ReturnFailedOutcome:
                    return ResultValue<JobDetails>.Failure("FakeBackgroundJobProvider faking an error");
                case FakeBackgroundJobProviderBehaviour.ReturnNull:
                    return null!;
                case FakeBackgroundJobProviderBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    return ResultValue<JobDetails>.Failure($"Unknown Behaviour - {Behaviour}");
            }
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
            switch (Behaviour)
            {
                case FakeBackgroundJobProviderBehaviour.ReturnSuccessfulOutcome:
                    return ResultValue<JobDetails>.Succeed(new JobDetails("1", enqueueAt));
                case FakeBackgroundJobProviderBehaviour.ReturnFailedOutcome:
                    return ResultValue<JobDetails>.Failure("FakeBackgroundJobProvider faking an error");
                case FakeBackgroundJobProviderBehaviour.ReturnNull:
                    return null!;
                case FakeBackgroundJobProviderBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    return ResultValue<JobDetails>.Failure($"Unknown Behaviour - {Behaviour}");
            }
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
        public Result AddOrUpdateRecurringJob<T>(Expression<Action<T>> methodCall, string jobName, string cronExpression,TimeZoneInfo timeZoneInfo, string queueName = "default")
        {
            switch (Behaviour)
            {
                case FakeBackgroundJobProviderBehaviour.ReturnSuccessfulOutcome:
                    return Result.Success();
                case FakeBackgroundJobProviderBehaviour.ReturnFailedOutcome:
                    return Result.Failure("Failed");
                case FakeBackgroundJobProviderBehaviour.ReturnNull:
                    return null!;
                case FakeBackgroundJobProviderBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    return Result.Failure($"Unknown Behaviour - {Behaviour}");
            }
        }

        /// <summary>
        /// RemoveIfExists
        /// </summary>
        /// <param name="jobName"></param>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="Exception"></exception>
        public Result RemoveRecurringJob(string jobName)
        {
            switch (Behaviour)
            {
                case FakeBackgroundJobProviderBehaviour.ReturnSuccessfulOutcome:
                    return Result.Success();
                case FakeBackgroundJobProviderBehaviour.ReturnFailedOutcome:
                    return Result.Success();
                case FakeBackgroundJobProviderBehaviour.ReturnNull:
                    return Result.Success();
                case FakeBackgroundJobProviderBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    return Result.Failure($"Unknown Behaviour - {Behaviour}");
            }
        }
    }

    /// <summary>
    /// Fake behaviour enum
    /// </summary>
    public enum FakeBackgroundJobProviderBehaviour
    {
        /// <summary>
        /// Succeed
        /// </summary>
        ReturnSuccessfulOutcome,
        
        /// <summary>
        /// Fail
        /// </summary>
        ReturnFailedOutcome,
        
        /// <summary>
        /// Return null
        /// </summary>
        ReturnNull,
        
        /// <summary>
        /// Throw an Exception
        /// </summary>
        ThrowException
    }
    
}