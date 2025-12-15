namespace Odin.System
{

    /// <summary>
    /// Represents the outcome of an operation that was successful or failed,
    /// together with a list of Messages.
    /// </summary>
    public record Result : Result<string>
    {
        /// <inheritdoc />
        public Result() 
        {
        }
        
        /// <summary>
        /// Default constructor.
        /// Use ResultValue.Succeed() for a successful Outcome with no message.
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        public Result(bool isSuccess, string? message = null) : base(isSuccess, message)
        {
        }

        /// <inheritdoc />
        public Result(bool isSuccess, IEnumerable<string>? messages = null) : base(isSuccess, messages)
        {
        }

        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static Result Failure(string? message)
        {
            return new Result(false, message);
        }
        
        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static Result Failure(IEnumerable<string> messages)
        {
            return new Result(false, messages);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <returns></returns>
        public new static Result Success()
        {
            return new Result(true, null as string);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static Result Success(string? message)
        {
            return new Result(true, message);
        }
        
        /// <summary>
        /// Success
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static Result Success(IEnumerable<string> messages)
        {
            return new Result(true, messages);
        }
    }
}