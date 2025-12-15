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

        /// <summary>
        /// Specifies a precondition that must hold true when the enclosing method is called.
        /// </summary>
        /// <param name="precondition">The precondition that is required to be <c>true</c>.</param>
        /// <param name="userMessage">Optional English description of what the precondition is.</param>
        /// <param name="conditionText">Optional pseudo-code representation of the condition expression.</param>
        /// <exception cref="ContractException">
        /// Thrown when <paramref name="precondition"/> is <c>false</c>.
        /// </exception>
        public static void Requires(bool precondition, string? userMessage = null, string? conditionText = null)
        {
            if (!precondition) ReportFailure(ContractFailureKind.Precondition, userMessage, conditionText);
        }

        /// <summary>
        /// Requires that argument be not null. If it is, raises an ArgumentNullException.
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="userMessage">Defaults to 'Argument must not be null'</param>
        /// <param name="conditionText">Optional pseudo-code representation of the not null expression.</param>
        public static void RequiresNotNull(object? argument, string? userMessage = "Argument must not be null"
            , string? conditionText = null)
        {
            Requires<ArgumentNullException>(argument != null, userMessage, conditionText);
        }

        /// <summary>
        /// Specifies a precondition that must hold true when the enclosing method is called
        /// and throws a specific exception type when the precondition fails.
        /// </summary>
        /// <typeparam name="TException">
        /// The type of exception to throw when the precondition fails.
        /// The type must have a public constructor that accepts a single <see cref="string"/> parameter.
        /// </typeparam>
        /// <param name="precondition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">Optional user readable message describing the precondition.</param>
        /// <param name="conditionText">Optional user readable message describing the precondition.</param>
        /// <exception cref="ContractException">
        /// Thrown when the specified exception type cannot be constructed.
        /// </exception>
        /// <exception cref="Exception">
        /// An instance of <typeparamref name="TException"/> when <paramref name="precondition"/> is <c>false</c>.
        /// </exception>
        public static void Requires<TException>(bool precondition, string? userMessage = null,
            string? conditionText = null)
            where TException : Exception
        {
            if (precondition) return;

            // Try to honor the requested exception type first.
            string message = BuildFailureMessage(ContractFailureKind.Precondition, userMessage, conditionText);

            Exception? exception = null;
            try
            {
                exception = (Exception?)Activator.CreateInstance(typeof(TException), message);
            }
            catch
            {
                // Swallow and fall back to ContractException.
            }

            if (exception is not null)
            {
                throw exception;
            }

            // Fall back to standard handling if we cannot construct TException.
            ReportFailure(ContractFailureKind.Precondition, userMessage, conditionText: null);
        }


        private static void ReportFailure(ContractFailureKind kind, string? userMessage, string? conditionText)
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

        private static string BuildFailureMessage(ContractFailureKind kind, string? userMessage, string? conditionText)
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