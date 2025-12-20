namespace Odin.System
{
    /// <summary>
    /// Represents the success or failure of an operation that returns a Value\Result on success,
    /// and list of detailed 'ResultMessage2' Messages
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>To be renamed to ResultValue2 of TValue</remarks>
    public record ResultValue2<TValue> : ResultValue<TValue, ResultMessage2>
    {
        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public ResultValue2()
        {
            IsSuccess = false;
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isSuccess">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="messages">Optional, but good practice is to provide messages for failed results.</param>
        public ResultValue2(bool isSuccess, TValue? value, IEnumerable<ResultMessage2>? messages)
        {
            Assertions.RequiresArgumentPrecondition(!(value == null && isSuccess), "Value is required for a successful result.");
            Value = value;
            _messages = messages?.ToList();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isSuccess">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="message">Optional, but good practice is to provide messages for failed results.</param>
        public ResultValue2(bool isSuccess, TValue? value, ResultMessage2? message = null)
        {
            Assertions.RequiresArgumentPrecondition(!(value == null && isSuccess), "A value is required for a successful result.");
            IsSuccess = isSuccess;
            Value = value;
            _messages = message != null ? [message] : null;
        }
        
        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="messages">Normally included as best practice for failed operations, but not mandatory.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public new static ResultValue2<TValue> Failure(IEnumerable<ResultMessage2> messages, TValue? value = default(TValue) )
        {
            return new ResultValue2<TValue>(false, value, messages);
        }

        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="message">Required for failed operations.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public new static ResultValue2<TValue> Failure(ResultMessage2 message, TValue? value = default(TValue) )
        {
            return new ResultValue2<TValue>(false, value, new List<ResultMessage2>() { message });
        }
        
        /// <summary>
        /// Creates a successful Result with Value set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static ResultValue2<TValue> Success(TValue value)
        {
            return new ResultValue2<TValue>(true, value, null as ResultMessage2);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set and a single Message.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static ResultValue2<TValue> Success(TValue value, ResultMessage2? message)
        {
            return new ResultValue2<TValue>(true, value, message);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set, and several Messages.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static ResultValue2<TValue> Success(TValue value, IEnumerable<ResultMessage2> messages)
        {
            return new ResultValue2<TValue>(true, value, messages);
        }
    }
}