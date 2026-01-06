namespace Odin.System
{
    /// <summary>
    /// Represents the success or failure of an operation that returns a Value\Result on success,
    /// and list of detailed 'MessageEx' messages also containing a message severity and optionally
    /// an exception.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>To be renamed to ResultValueEx of TValue</remarks>
    public class ResultValueEx<TValue> : ResultValue<TValue, MessageEx> where TValue : notnull
    {
        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        public ResultValueEx()
        {
            IsSuccess = false;
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isSuccess">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="messages">Optional, but good practice is to provide messages for failed results.</param>
        public ResultValueEx(bool isSuccess, TValue? value, IEnumerable<MessageEx>? messages)
        {
            Precondition.Requires(!(value == null && isSuccess), "Value is required for a successful result.");
            Value = value;
            _messages = messages?.ToList();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isSuccess">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="message">Optional, but good practice is to provide messages for failed results.</param>
        public ResultValueEx(bool isSuccess, TValue? value, MessageEx? message = null)
        {
            Precondition.Requires(!(value == null && isSuccess), "A value is required for a successful result.");
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
        public new static ResultValueEx<TValue> Failure(IEnumerable<MessageEx> messages, TValue? value = default(TValue) )
        {
            return new ResultValueEx<TValue>(false, value, messages);
        }

        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="message">Required for failed operations.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public new static ResultValueEx<TValue> Failure(MessageEx message, TValue? value = default(TValue) )
        {
            return new ResultValueEx<TValue>(false, value, new List<MessageEx>() { message });
        }
        
        /// <summary>
        /// Creates a successful Result with Value set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static ResultValueEx<TValue> Success(TValue value)
        {
            return new ResultValueEx<TValue>(true, value, null as MessageEx);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set and a single Message.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public new static ResultValueEx<TValue> Success(TValue value, MessageEx? message)
        {
            return new ResultValueEx<TValue>(true, value, message);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set, and several Messages.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public new static ResultValueEx<TValue> Success(TValue value, IEnumerable<MessageEx> messages)
        {
            return new ResultValueEx<TValue>(true, value, messages);
        }
    }
}