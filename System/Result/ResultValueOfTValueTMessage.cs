namespace Odin.System
{
    /// <summary>
    /// Represents the success or failure of an operation that returns a Value\Result on success,
    /// and list of messages, of type TMessage.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public record ResultValue<TValue, TMessage> where TMessage : class
    {
        /// <summary>
        /// True if successful
        /// </summary>
        public bool IsSuccess { get; init; }
        
        /// <summary>
        /// Value is typically set when Success is True.
        /// Value is null when Success is false.
        /// </summary>
        public TValue? Value { get; init; }
        
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
        /// Parameterless constructor for serialisation, etc.
        /// </summary>
        public ResultValue()
        {
            Value = default(TValue);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="success">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="messages">Optional, but good practice is to provide messages for failed results.</param>
        protected ResultValue(bool success, TValue? value, IEnumerable<TMessage>? messages)
        {
            Assertions.RequiresArgumentPrecondition(!(value == null && success), "Value is required for a successful result.");
            Value = value;
            _messages = messages?.ToList();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="success">true or false</param>
        /// <param name="value">Required if successful</param>
        /// <param name="message">Optional, but good practice is to provide messages for failed results.</param>
        protected ResultValue(bool success, TValue? value, TMessage? message = null)
        {
            Assertions.RequiresArgumentPrecondition(!(value == null && success), "Value is required for a successful result.");
            Value = value;
            _messages = message != null ? [message] : null;
        }

        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="value">Required.</param>
        /// <param name="messages">Not normally used for successful operations, but can be for informational purposes.</param>
        /// <returns></returns>
        public static ResultValue<TValue, TMessage> Success(TValue value, IEnumerable<TMessage>? messages)
        {
            Assertions.RequiresArgumentNotNull(value);
            return new ResultValue<TValue, TMessage>(true, value, messages);    
        }

        /// <summary>
        /// Creates a successful Result with Value set.
        /// </summary>
        /// <param name="value">Required.</param>
        /// <returns></returns>
        public static ResultValue<TValue, TMessage> Success(TValue value)
        {
            Assertions.RequiresArgumentNotNull(value);
            return new ResultValue<TValue, TMessage>(true, value, null as TMessage);
        }
        
        /// <summary>
        /// Creates a successful Result with Value set, and 1 Message item.
        /// </summary>
        /// <param name="value">Required.</param>
        /// <param name="message">Not normally used for successful operations, but can be for informational purposes.</param>
        /// <returns></returns>
        public static ResultValue<TValue, TMessage> Success(TValue value, TMessage? message)
        {
            Assertions.RequiresArgumentNotNull(value);
            return new ResultValue<TValue, TMessage>(true, value, message);
        }

        /// <summary>
        /// Creates a successful Result with Value set, and several Messages.
        /// </summary>
        /// <param name="messages">Normally included as best practice for failed operations, but not mandatory.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public static ResultValue<TValue, TMessage> Failure(IEnumerable<TMessage> messages, TValue? value = default(TValue) )
        {
            return new ResultValue<TValue, TMessage>(false, value, messages);
        }

        /// <summary>
        /// Success.
        /// </summary>
        /// <param name="message">Required for failed operations.</param>
        /// <param name="value">Normally null\default for a failure, but not necessarily.</param>
        /// <returns></returns>
        public static ResultValue<TValue, TMessage> Failure(TMessage message, TValue? value = default(TValue) )
        {
            return new ResultValue<TValue, TMessage>(false, value, new List<TMessage>() { message });
        }
    }
}