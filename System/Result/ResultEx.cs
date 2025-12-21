using Microsoft.Extensions.Logging;

namespace Odin.System
{

    /// <summary>
    /// Like Result, but with a list of Messages of type ResultMessage2
    /// which includes a Severity, a Message and optionally an Exception.
    /// </summary>
    public record ResultEx : Result<MessageEx>
    {
        /// <inheritdoc />
        public ResultEx() 
        {
        }
        
        /// <summary>
        /// Default constructor.
        /// Use ResultValue.Succeed() for a successful Outcome with no message.
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        public ResultEx(bool isSuccess, MessageEx? message = null) : base(isSuccess, message)
        {
        }

        /// <inheritdoc />
        public ResultEx(bool isSuccess, IEnumerable<MessageEx>? messages = null) : base(isSuccess, messages)
        {
        }

        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static ResultEx Failure(MessageEx message)
        {
            return new ResultEx(false, message);
        }
        
        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static ResultEx Failure(string? message, LogLevel severity = LogLevel.Error, Exception? error = null)
        {
            return new ResultEx(false, new MessageEx() { Message = message, Severity = severity, Error = error });
        }
        
        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static ResultEx Failure(IEnumerable<MessageEx> messages)
        {
            return new ResultEx(false, messages);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <returns></returns>
        public new static ResultEx Success()
        {
            return new ResultEx(true, null as MessageEx);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static ResultEx Success(MessageEx? message)
        {
            return new ResultEx(true, message);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResultEx Success(string message)
        {
            return new ResultEx(true, new MessageEx(){Message = message,  Severity = LogLevel.Information});
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static ResultEx Success(IEnumerable<MessageEx> messages)
        {
            return new ResultEx(true, messages);
        }
        
        /// <summary>
        /// Returns Success only if all succeed, else returns the first failure.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static ResultEx Combine(params ResultEx[] results)
        {
            foreach (ResultEx result in results)
            {
                if (!result.IsSuccess)
                    return result;
            }

            return Success();
        }
    }
}