namespace Odin.DesignContracts
{
    /// <summary>
    /// Provides design-time contracts for runtime validation of preconditions,
    /// postconditions, and object invariants.
    /// </summary>
    /// <remarks>
    /// This class is intentionally similar in surface area to
    /// <c>System.Diagnostics.Contracts.Contract</c> from the classic .NET Framework,
    /// but it is implemented independently under the <c>Odin.DesignContracts</c> namespace.
    /// </remarks>
    public static class Contract
    {
        /// <summary>
        /// Occurs when a contract fails and before a <see cref="ContractException"/> is thrown.
        /// </summary>
        /// <remarks>
        /// Event handlers may set <see cref="ContractFailedEventArgs.Handled"/> to <c>true</c>
        /// to suppress the default exception behavior and perform custom handling instead.
        /// </remarks>
        public static event EventHandler<ContractFailedEventArgs>? ContractFailed;

        internal static void ReportFailure(ContractFailureKind kind, string? userMessage, string? conditionText)
        {
            string message = BuildFailureMessage(kind, userMessage, conditionText);
            ContractFailedEventArgs args = new ContractFailedEventArgs(kind, message, userMessage, conditionText);
            ContractFailed?.Invoke(null, args);
            if (args.Handled)
            {
                // A handler chose to manage the failure; do not throw by default.
                return;
            }

            throw new ContractException(kind, message, userMessage, conditionText);
        }

        internal static string GetKindFailedText(ContractFailureKind kind)
        {
            switch (kind)
            {
                case ContractFailureKind.Precondition:
                    return "Precondition not met";
                case ContractFailureKind.Postcondition:
                    return "Postcondition not honoured";
                case ContractFailureKind.Invariant:
                    return "Invariant broken";
                case ContractFailureKind.Assertion:
                    return "Assertion failed";
                case ContractFailureKind.Assumption:
                    return "Assumption failed";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
        
        internal static string BuildFailureMessage(ContractFailureKind kind, string? userMessage, string? conditionText)
        {
            if (!string.IsNullOrWhiteSpace(userMessage) && !string.IsNullOrWhiteSpace(conditionText))
            {
                return $"{GetKindFailedText(kind)}: {userMessage} [Condition: {conditionText}]";
            }

            if (!string.IsNullOrWhiteSpace(userMessage))
            {
                return $"{GetKindFailedText(kind)}: {userMessage}";
            }

            if (!string.IsNullOrWhiteSpace(conditionText))
            {
                return $"{GetKindFailedText(kind)}: {conditionText}";
            }

            return $"{GetKindFailedText(kind)}.";
        }

        /// <summary>
        /// Specifies an object invariant that must hold true whenever the object is in a valid state.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the invariant.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// Invariants are evaluated only when <see cref="ContractRuntime.InvariantsEnabled"/> is <c>true</c>.
        /// Calls to this method become no-ops when invariants are disabled.
        /// It is expected that source-generated code will invoke invariant methods
        /// (marked with <see cref="ClassInvariantMethodAttribute"/>) at appropriate points.
        /// </remarks>
        public static void Invariant(bool condition, string? userMessage = null, string? conditionText = null)
        {
            if (!DesignContractOptions.Current.EnableInvariants)
            {
                return;
            }

            if (!condition)
            {
                Contract.ReportFailure(
                    ContractFailureKind.Invariant,
                    userMessage,
                    conditionText);
            }
        }
        
        /// <summary>
        /// Specifies an assertion that must hold true at the given point in the code.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the assertion.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// Assertions are always evaluated at runtime.
        /// </remarks>
        public static void Assert(bool condition, string? userMessage = null, string? conditionText = null)
        {
            if (!condition)
            {
                Contract.ReportFailure(
                    ContractFailureKind.Assertion,
                    userMessage,
                    conditionText);
            }
        }

        /// <summary>
        /// Specifies an assumption that the analysis environment may rely on.
        /// </summary>
        /// <param name="condition">The condition that is assumed to be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the assumption.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// At runtime, <see cref="Assume"/> behaves identically to <see cref="Assert"/>,
        /// but analyzers may interpret assumptions differently.
        /// </remarks>
        public static void Assume(bool condition, string? userMessage = null, string? conditionText = null)
        {
            if (!condition)
            {
                Contract.ReportFailure(
                    ContractFailureKind.Assumption,
                    userMessage,
                    conditionText);
            }
        }
    }
}