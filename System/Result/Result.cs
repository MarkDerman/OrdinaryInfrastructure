namespace Odin.System
{
    /// <summary>
    /// Represents the result of an operation that can succeed or fail,
    /// with a list of messages of type TMessage.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public record Result<TMessage> where TMessage : class
    {
        /// <summary>
        /// True if successful
        /// </summary>
        public bool IsSuccess { get; init; }
        
        /// <summary>
        /// Messages list
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected List<TMessage>? _messages;

        /// <summary>
        /// Messages
        /// </summary>
        public IReadOnlyList<TMessage> Messages
        {
            get
            {
                _messages ??= new List<TMessage>();
                return _messages;
            }
            init  // For deserialisation
            {
                _messages = value.ToList();
            }
        }
        
        /// <summary>
        /// All messages flattened into 1 message.
        /// Assumes a decent implementation of TMessage.ToString()
        /// </summary>
        public string MessagesToString(string separator = " | ")
        {
            if (_messages == null || _messages.Count == 0)
            {
                return string.Empty;
            }
            return string.Join(separator, _messages.Select(c => c.ToString()));
        }

        /// <summary>
        /// Default constructor.
        /// Note that IsSuccess defaults to false.
        /// </summary>
        public Result()
        {
            IsSuccess = false;
        }
        
        /// <summary>
        /// Result constructor.
        /// </summary>
        /// <param name="isSuccess">True or False</param>
        /// <param name="message">Optional message. Best practice is to include at least 1 message for failed operations however.</param>
        public Result(bool isSuccess, TMessage? message = null)
        {
            IsSuccess = isSuccess;
            if (message != null)
            {
                _messages = new List<TMessage> { message };
            }
        }


        /// <summary>
        /// Result constructor.
        /// </summary>
        /// <param name="isSuccess">True or False</param>
        /// <param name="messages">Optional message. Best practice is to include at least 1 message for failed operations however.</param>
        public Result(bool isSuccess, IEnumerable<TMessage>? messages)
        {
            IsSuccess = isSuccess;
            if (messages != null)
            {
                _messages = messages.ToList();
            }
        }

        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<TMessage> Failure(TMessage message)
        {
            return new Result<TMessage>(false, message);
        }

        /// <summary>
        /// Creates a Failure outcome with messages.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns> 
        public static Result<TMessage> Failure(IEnumerable<TMessage> messages)
        {
            return new Result<TMessage>(false, messages);
        }
        
        /// <summary>
        /// Success.
        /// </summary>
        /// <returns></returns>
        public static Result<TMessage> Success()
        {
            return new Result<TMessage>(true);
        }
        
        /// <summary>
        /// Success, optionally including a message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<TMessage> Success(TMessage? message)
        {
            return new Result<TMessage>(true, message);
        }

        /// <summary>
        /// Success, including messages
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static Result<TMessage> Success(IEnumerable<TMessage> messages)
        {
            return new Result<TMessage>(true, messages);
        }

        /// <summary>
        /// Returns Success only if all succeed, else returns the first failure.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result<TMessage> Combine(params Result<TMessage>[] results)
        {
            foreach (Result<TMessage> result in results)
            {
                if (!result.IsSuccess)
                    return result;
            }

            return Success();
        }
    }

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

    internal static class Assertions
    {
        internal static void RequiresArgumentPrecondition(bool requirement, string argumentRequirementMessage)
        {
            if (!requirement) throw new ArgumentException(argumentRequirementMessage);
        }

        internal static void RequiresArgumentNotNull(object? argument, string? argumentIsRequiredMessage = null)
        {
            if (argument != null) return;
            if (string.IsNullOrWhiteSpace(argumentIsRequiredMessage))
            {
                argumentIsRequiredMessage = $"{nameof(argument)} is required";
            }
            ArgumentNullException ex = new ArgumentNullException(argumentIsRequiredMessage);
            throw ex;
        }
    }
}