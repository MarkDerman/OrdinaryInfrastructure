using Microsoft.Extensions.Logging;

namespace Odin.System
{

    /// <summary>
    /// Like Result, but with a list of Messages of type ResultMessage2
    /// which includes a Severity, a Message and optionally an Exception.
    /// </summary>
    public record Result2 : Result<ResultMessage2>
    {
        /// <inheritdoc />
        public Result2() 
        {
        }
        
        /// <summary>
        /// Default constructor.
        /// Use ResultValue.Succeed() for a successful Outcome with no message.
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        public Result2(bool isSuccess, ResultMessage2? message = null) : base(isSuccess, message)
        {
        }

        /// <inheritdoc />
        public Result2(bool isSuccess, IEnumerable<ResultMessage2>? messages = null) : base(isSuccess, messages)
        {
        }

        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static Result2 Failure(ResultMessage2 message)
        {
            return new Result2(false, message);
        }
        
        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Result2 Failure(string? message, LogLevel severity = LogLevel.Error, Exception? error = null)
        {
            return new Result2(false, new ResultMessage2() { Message = message, Severity = severity, Error = error });
        }
        
        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static Result2 Failure(IEnumerable<ResultMessage2> messages)
        {
            return new Result2(false, messages);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <returns></returns>
        public new static Result2 Success()
        {
            return new Result2(true, null as ResultMessage2);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static Result2 Success(ResultMessage2? message)
        {
            return new Result2(true, message);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result2 Success(string message)
        {
            return new Result2(true, new ResultMessage2(){Message = message,  Severity = LogLevel.Information});
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static Result2 Success(IEnumerable<ResultMessage2> messages)
        {
            return new Result2(true, messages);
        }
        
        /// <summary>
        /// Returns Success only if all succeed, else returns the first failure.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result2 Combine(params Result2[] results)
        {
            foreach (Result2 result in results)
            {
                if (!result.IsSuccess)
                    return result;
            }

            return Success();
        }
    }
}