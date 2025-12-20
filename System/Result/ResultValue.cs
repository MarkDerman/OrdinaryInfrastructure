namespace Odin.System
{
    /// <summary>
    /// Represents the success or failure of an operation that returns a Value\Result on success,
    /// and list of string messages.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>To be renamed to ResultValue of TValue</remarks>
    public record ResultValue<TValue> : ResultValue<TValue, string>
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
        public ResultValue(bool isSuccess, TValue? value, IEnumerable<string>? messages)
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
        public ResultValue(bool isSuccess, TValue? value, string? message = null)
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
        public new static ResultValue<TValue> Failure(IEnumerable<string> messages, TValue? value = default(TValue) )
        {
            return new ResultValue<TValue>(false, value, messages);
        }

        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="message">Required for failed operations.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public new static ResultValue<TValue> Failure(string message, TValue? value = default(TValue) )
        {
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
    }
}