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
        /// <param name="message">Optional message. Best practice is to include at least 1 message for failed operations, however this is not enforced.</param>
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
        /// <param name="messages">Optional messages. Normal practice is to include at least 1 message for failed operations.</param>
        public Result(bool isSuccess, IEnumerable<TMessage>? messages)
        {
            IsSuccess = isSuccess;
            if (messages != null)
            {
                _messages = messages.ToList();
            }
        }

        /// <summary>
        ///  Creates a failed result (IsSuccess==false) with a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<TMessage> Failure(TMessage message)
        {
            return new Result<TMessage>(false, message);
        }

        /// <summary>
        /// Creates a failed result (IsSuccess==false) with messages.
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
}