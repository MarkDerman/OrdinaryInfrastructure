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
        /// <param name="message">Must be not null or whitespace</param>
        /// <returns></returns>
        public new static Result Failure(string message)
        {
            Precondition.Requires(!string.IsNullOrWhiteSpace(message), $"{nameof(message)} is required.");
            return new Result(false, message);
        }
        
        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="messages">Requires at least 1 not null or whitespace message</param>
        /// <returns></returns>
        public new static Result Failure(IEnumerable<string> messages)
        {
            Precondition.RequiresNotNull(messages);
            List<string> list = messages.ToList();
            Precondition.Requires(list.Any(s => !string.IsNullOrWhiteSpace(s)),"At least 1 message is required.");
            return new Result(false, list);
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
        
        /// <summary>
        /// Returns Success only if all succeed, else returns the first failure.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result Combine(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (!result.IsSuccess)
                    return result;
            }

            return Success();
        }
    }
}