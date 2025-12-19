namespace Odin.DesignContracts
{
    /// <summary>
    /// Provides data for the <see cref="Postcondition.ContractFailed"/> event.
    /// </summary>
    public sealed class ContractFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractFailedEventArgs"/> class.
        /// </summary>
        /// <param name="kind">The category of the contract failure.</param>
        /// <param name="message">The fully formatted failure message.</param>
        /// <param name="userMessage">The user-visible message associated with the contract, if any.</param>
        /// <param name="conditionText">The text representation of the condition that failed, if provided.</param>
        public ContractFailedEventArgs(
            ContractFailureKind kind,
            string message,
            string? userMessage,
            string? conditionText)
        {
            Kind = kind;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            UserMessage = userMessage;
            ConditionText = conditionText;
        }

        /// <summary>
        /// Gets the category of the contract failure.
        /// </summary>
        public ContractFailureKind Kind { get; }

        /// <summary>
        /// Gets the fully formatted failure message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the user-visible message associated with the contract, if any.
        /// </summary>
        public string? UserMessage { get; }

        /// <summary>
        /// Gets the text representation of the condition that failed, if supplied.
        /// </summary>
        public string? ConditionText { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the failure has been handled
        /// by an event subscriber and the default behavior (throwing) should be suppressed.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application should be unwound
        /// (for example, by rethrowing the resulting <see cref="ContractException"/>).
        /// </summary>
        /// <remarks>
        /// This property is provided for advanced scenarios. Typical usage is to leave it as <c>false</c>.
        /// </remarks>
        public bool Unwind { get; set; }
    }
}
