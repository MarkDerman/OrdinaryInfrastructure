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
    public static partial class Contract
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

        internal static string BuildFailureMessage(ContractFailureKind kind, string? userMessage, string? conditionText)
        {
            string kindText = kind.ToString();

            if (!string.IsNullOrWhiteSpace(userMessage) && !string.IsNullOrWhiteSpace(conditionText))
            {
                return $"{kindText} failed: {userMessage} [Condition: {conditionText}]";
            }

            if (!string.IsNullOrWhiteSpace(userMessage))
            {
                return $"{kindText} failed: {userMessage}";
            }

            if (!string.IsNullOrWhiteSpace(conditionText))
            {
                return $"{kindText} failed: {conditionText}";
            }

            return $"{kindText} failed.";
        }
    }
}