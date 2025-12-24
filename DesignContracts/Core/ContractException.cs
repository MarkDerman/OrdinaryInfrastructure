namespace Odin.DesignContracts
{
    /// <summary>
    /// Represents an error that occurs when a design-time contract is violated at runtime.
    /// </summary>
    [Serializable]
    public sealed class ContractException : Exception
    {
        /// <summary>
        /// Precondition, Postcondition, Invariant, Assertion or Assumption
        /// </summary>
        public ContractKind Kind { get; }

        /// <summary>
        /// Gets the text representation of the condition that failed, if supplied.
        /// </summary>
        public string? ConditionText { get; }

        /// <summary>
        /// Gets the user-visible message that was associated with the contract, if supplied.
        /// </summary>
        public string? UserMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractException"/> class.
        /// </summary>
        /// <param name="kind">The category of the contract failure.</param>
        /// <param name="message">The fully formatted failure message.</param>
        /// <param name="userMessage">The user-visible message associated with the contract, if any.</param>
        /// <param name="conditionText">The text representation of the condition that failed, if provided.</param>
        public ContractException(
            ContractKind kind,
            string message,
            string? userMessage = null,
            string? conditionText = null)
            : base(message)
        {
            Kind = kind;
            UserMessage = userMessage;
            ConditionText = conditionText;
        }

    }
}
