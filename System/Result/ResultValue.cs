namespace Odin.System
{
    /// <summary>
    /// Represents the success or failure of an operation that returns a non-null Value\Result on success,
    /// and list of string messages.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>To be renamed to ResultValue of TValue</remarks>
    public class ResultValue<TValue> : ResultValue<TValue, string> where TValue : notnull
    {
        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public ResultValue()
        {
            IsSuccess = false;
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isSuccess">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="messages">Optional, but good practice is to provide messages for failed results.</param>
        public ResultValue(bool isSuccess, TValue? value, IEnumerable<string>? messages) : base(isSuccess, value, messages)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isSuccess">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="message">Optional, but good practice is to provide messages for failed results.</param>
        public ResultValue(bool isSuccess, TValue? value, string? message = null) : base(isSuccess, value, message)
        {
        }
        
        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="messages">Normally included as best practice for failed operations, but not mandatory.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public new static ResultValue<TValue> Failure(IEnumerable<string> messages, TValue? value = default(TValue) )
        {
            Precondition.RequiresNotNull(messages);
            List<string> list = messages.ToList();
            Precondition.Requires(list.Any(s => !string.IsNullOrWhiteSpace(s)),"At least 1 message is required.");
            return new ResultValue<TValue>(false, value, list);
        }

        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="message">Required for failed operations.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public new static ResultValue<TValue> Failure(string message, TValue? value = default(TValue) )
        {
            Precondition.Requires(!string.IsNullOrWhiteSpace(message), $"{nameof(message)} is required.");
            return new ResultValue<TValue>(false, value, new List<string>() { message });
        }
        
        /// <summary>
        /// Creates a successful Result with Value set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static ResultValue<TValue> Success(TValue value)
        {
            return new ResultValue<TValue>(true, value, null as string);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set and a single Message.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static ResultValue<TValue> Success(TValue value, string? message)
        {
            return new ResultValue<TValue>(true, value, message);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set, and several Messages.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static ResultValue<TValue> Success(TValue value, IEnumerable<string> messages)
        {
            return new ResultValue<TValue>(true, value, messages);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOtherValue"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public override ResultValue<TOtherValue> ToFailedResult<TOtherValue>()
        {
            if (IsSuccess)
            {
                throw new ArgumentException($"Cannot convert a successful result of type {GetType().FullName} " +
                                            $"to a failed result of type {typeof(ResultValue<TOtherValue>).FullName}.");
            }
            
            return ResultValue<TOtherValue>.Failure(Messages);
        }

        /// <summary>
        /// Projects the value when successful.
        /// Failures are propagated unchanged.
        /// </summary>
        /// <typeparam name="TOtherValue"></typeparam>
        /// <param name="map"></param>
        /// <returns></returns>
        public new ResultValue<TOtherValue> Map<TOtherValue>(Func<TValue, TOtherValue> map) where TOtherValue : notnull
        {
            Precondition.RequiresNotNull(map);

            return IsSuccess
                ? ResultValue<TOtherValue>.Success(map(Value))
                : ResultValue<TOtherValue>.Failure(Messages);
        }

        /// <summary>
        /// Chains the next result-producing operation when successful.
        /// Failures are propagated unchanged.
        /// </summary>
        /// <typeparam name="TOtherValue"></typeparam>
        /// <param name="bind"></param>
        /// <returns></returns>
        public ResultValue<TOtherValue> Bind<TOtherValue>(Func<TValue, ResultValue<TOtherValue>> bind)
            where TOtherValue : notnull
        {
            Precondition.RequiresNotNull(bind);

            return IsSuccess
                ? bind(Value)
                : ResultValue<TOtherValue>.Failure(Messages);
        }

        /// <summary>
        /// Executes the action when successful and returns the same instance.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public new ResultValue<TValue> Tap(Action<TValue> onSuccess)
        {
            Precondition.RequiresNotNull(onSuccess);

            if (IsSuccess)
            {
                onSuccess(Value);
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
        public new TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<IReadOnlyList<string>, TResult> onFailure)
        {
            return base.Match(onSuccess, onFailure);
        }
    }
}
