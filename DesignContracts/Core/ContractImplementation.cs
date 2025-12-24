namespace Odin.DesignContracts
{
    /// <summary>
    /// Provides an implemented edition of the postconditions Ensures call.
    /// </summary>
    public static class ContractImplementation
    {
        /// <summary>
        /// Specifies a postcondition that must hold true when the enclosing method returns.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the postcondition.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// Postconditions are evaluated only when <see cref="ContractOptions.Postconditions"/> is not <c>Bypass</c>.
        /// Calls to this method are no-ops when postconditions are disabled.
        /// It is expected that source-generated code will invoke this method at
        /// appropriate points (typically immediately before method exit).
        /// </remarks>
        public static void Ensures(bool condition, string? userMessage = null, string? conditionText = null)
        {
            Contract.HandleContractCondition(ContractKind.Postcondition, condition, userMessage, conditionText);
        }
    }
}