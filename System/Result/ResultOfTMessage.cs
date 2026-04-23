using System.Text.Json.Serialization;

namespace Odin.System
{
    /// <summary>
    /// Represents the result of an operation that can succeed or fail,
    /// with a list of messages of type TMessage.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class Result<TMessage> where TMessage : class
    {
        /// <summary>
        /// True if successful
        /// </summary>
        [JsonPropertyOrder(-2)]
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Messages list
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected List<TMessage>? _messages;

        /// <summary>
        /// Messages
        /// </summary>
        [JsonPropertyOrder(0)]
        public IReadOnlyList<TMessage> Messages
        {
            get
            {
                _messages ??= new List<TMessage>();
                return _messages;
            }
            init // For deserialisation
            {
                _messages = value?.ToList();
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
            Precondition.RequiresNotNull(message);
            return new Result<TMessage>(false, message);
        }

        /// <summary>
        /// Creates a failed result (IsSuccess==false) with messages.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns> 
        public static Result<TMessage> Failure(IEnumerable<TMessage> messages)
        {
            Precondition.RequiresNotNull(messages);
            List<TMessage> messagesList = messages.ToList();
            Precondition.Requires(messagesList.Any(m => m != null!), "At least 1 message is required.");
            return new Result<TMessage>(false, messagesList);
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

        /// <summary>
        /// Returns success only if all succeed, otherwise aggregates the
        /// messages from every failed result.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static Result<TMessage> CombineAll(params Result<TMessage>[] results)
        {
            Precondition.RequiresNotNull(results);

            List<TMessage> failureMessages = results
                .Where(r => r is { IsSuccess: false })
                .SelectMany(r => r.Messages)
                .ToList();

            return failureMessages.Count == 0
                ? Success()
                : Failure(failureMessages);
        }

        /// <summary>
        /// Executes the action when the result is successful and returns the same instance.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public Result<TMessage> Tap(Action onSuccess)
        {
            Precondition.RequiresNotNull(onSuccess);

            if (IsSuccess)
            {
                onSuccess();
            }

            return this;
        }

        /// <summary>
        /// Matches the success or failure branch and returns the projected value.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public TResult Match<TResult>(Func<TResult> onSuccess, Func<IReadOnlyList<TMessage>, TResult> onFailure)
        {
            Precondition.RequiresNotNull(onSuccess);
            Precondition.RequiresNotNull(onFailure);

            return IsSuccess
                ? onSuccess()
                : onFailure(Messages);
        }

        /// <summary>
        /// Chains the next result-producing operation when successful.
        /// Failures are propagated unchanged.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public Result<TMessage> Bind(Func<Result<TMessage>> next)
        {
            Precondition.RequiresNotNull(next);

            return IsSuccess
                ? next()
                : Failure(Messages);
        }

        /// <summary>
        /// Chains the next result-with-value-producing operation when successful.
        /// Failures are propagated unchanged.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="next"></param>
        /// <returns></returns>
        public ResultValue<TValue, TMessage> Bind<TValue>(Func<ResultValue<TValue, TMessage>> next) where TValue : notnull
        {
            Precondition.RequiresNotNull(next);

            return IsSuccess
                ? next()
                : ResultValue<TValue, TMessage>.Failure(Messages);
        }
    }
}
