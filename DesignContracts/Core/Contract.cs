namespace Odin.DesignContracts
{
    /// <summary>
    /// Provides design-time contracts for runtime validation of preconditions,
    /// postconditions, class invariants, assertions and assumptions.
    /// </summary>
    /// <remarks>
    /// This class is intentionally similar in surface area to
    /// <c>System.Diagnostics.Contracts.Contract</c> from the classic .NET Framework,
    /// but it is implemented independently under the <c>Odin.DesignContracts</c> namespace.
    /// </remarks>
    public static class Contract
    {
        /// <summary>
        /// Specifies a precondition that must hold true when the enclosing method is called.
        /// </summary>
        /// <param name="precondition">The precondition that is required to be <c>true</c>.</param>
        /// <param name="userMessage">Optional English description of what the precondition is.</param>
        /// <param name="conditionText">Optional pseudocode representation of the condition expression.</param>
        /// <exception cref="ContractException">
        /// Thrown when <paramref name="precondition"/> is <c>false</c>.
        /// </exception>
        public static void Requires(bool precondition, string? userMessage = null, string? conditionText = null)
        {
            HandleContractCondition(ContractKind.Precondition, precondition, userMessage, conditionText);
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
            ContractHandlingBehaviour behaviour = ContractOptions.Current.Preconditions;
            if (behaviour == ContractHandlingBehaviour.Bypass)
            {
                return;
            }

            if (precondition) return;

            // Try to honor the requested exception type first.
            // Todo: Obey behaviour. Throw specific Exception only after event handling...
            string message = BuildFailureMessage(ContractKind.Precondition, userMessage, conditionText);

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
            ReportFailure(behaviour, ContractKind.Precondition, userMessage, conditionText: null);
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
            Requires(argument != null, userMessage, conditionText);
        }

        /// <summary>
        /// Specifies a postcondition that must hold true when the enclosing method returns.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the postcondition.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// Postconditions are evaluated only when <see cref="ContractOptions.Postconditions"/> are not set to <c>Bypass</c>.
        /// This method acts as a marker method for the Contracts Rewriter to instead insert calls to
        /// ContractImplementation.Ensures() at all method returns.
        /// </remarks>
        public static void Ensures(bool condition, string? userMessage = null, string? conditionText = null)
        {
        }

        
        /// <summary>
        /// Implementation of <see cref="Contract.Ensures"/>
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the postcondition.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        internal static void EnsuresImplementation(bool condition, string? userMessage = null, string? conditionText = null)
        {
            HandleContractCondition(ContractKind.Postcondition, condition, userMessage, conditionText);
        }
        
        /// <summary>
        /// Specifies a condition of the class that must hold true for the class to be in a valid state.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the invariant.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        public static void Invariant(bool condition, string? userMessage = null, string? conditionText = null)
        {
            HandleContractCondition(ContractKind.Invariant, condition, userMessage, conditionText);
        }

        /// <summary>
        /// Specifies an assertion that must hold true at the given point in the code.
        /// Only evaluated if ContractOptions.EnableAssertions is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the assertion.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// Assertions are always evaluated at runtime.
        /// </remarks>
        public static void Assert(bool condition, string? userMessage = null, string? conditionText = null)
        {
            HandleContractCondition(ContractKind.Assertion, condition, userMessage, conditionText);
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
            HandleContractCondition(ContractKind.Assumption, condition, userMessage, conditionText);
        }

        /// <summary>
        /// Occurs when a contract fails and before a <see cref="ContractException"/> is thrown.
        /// </summary>
        /// <remarks>
        /// Event handlers may set <see cref="ContractFailedEventArgs.Handled"/> to <c>true</c>
        /// to suppress the default exception behavior and perform custom handling instead.
        /// </remarks>
        public static event EventHandler<ContractFailedEventArgs>? ContractFailed;

        internal static void ResetContractFailedEventHandlers()
        {
            ContractFailed = null;
        }
        
        internal static void ReportFailure(ContractHandlingBehaviour handling, ContractKind kind,
            string? userMessage, string? conditionText)
        {
            // Note that if handling is inappropriately set to ByPass, then ReportFailure
            // does nothing which is fine.
            string message = BuildFailureMessage(kind, userMessage, conditionText);
            if (handling == ContractHandlingBehaviour.EventHandlersAndEscalation ||
                handling == ContractHandlingBehaviour.EventHandlersOnly)
            {
                if (ContractFailed is not null)
                {
                    ContractFailedEventArgs args = new
                        ContractFailedEventArgs(kind, message, userMessage, conditionText);
                    ContractFailed.Invoke(null, args);
                    if (args.Handled)
                    {
                        return; // A handler chose to manage the failure; do not throw in this case...
                    }
                }
            }

            if (handling == ContractHandlingBehaviour.EventHandlersAndEscalation ||
                handling == ContractHandlingBehaviour.EscalationOnly)
            {
                throw new ContractException(kind, message, userMessage, conditionText);
            }
        }

        internal static string GetKindFailedText(ContractKind kind)
        {
            switch (kind)
            {
                case ContractKind.Precondition:
                    return "Precondition not met";
                case ContractKind.Postcondition:
                    return "Postcondition not honoured";
                case ContractKind.Invariant:
                    return "Invariant broken";
                case ContractKind.Assertion:
                    return "Assertion failed";
                case ContractKind.Assumption:
                    return "Assumption failed";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        internal static string BuildFailureMessage(ContractKind kind, string? userMessage, string? conditionText)
        {
            bool blankUserMessage = string.IsNullOrWhiteSpace(userMessage); 
            bool blankConditionText = string.IsNullOrWhiteSpace(conditionText); 

            if (!blankUserMessage && !blankConditionText)
                return $"{GetKindFailedText(kind)}: {userMessage} [Condition: {conditionText}]";

            if (!blankUserMessage)
                return $"{GetKindFailedText(kind)}: {userMessage}";

            if (!blankConditionText)
                return $"{GetKindFailedText(kind)}: {conditionText}";

            return $"{GetKindFailedText(kind)}.";
        }

        internal static void HandleContractCondition(ContractKind kind, bool condition, string? userMessage = null, string? conditionText = null)
        {
            ContractHandlingBehaviour behaviour = ContractOptions.Current.GetBehaviourFor(kind);
            if (behaviour == ContractHandlingBehaviour.Bypass)
                return;
            if (!condition)
                ReportFailure(behaviour, kind, userMessage, conditionText);
        }


        /// <summary>
        /// Represents the return value of the enclosing method for use within postconditions.
        /// </summary>
        /// <typeparam name="T">The enclosing method return type.</typeparam>
        /// <returns>
        /// The value returned by the enclosing method.
        /// </returns>
        /// <remarks>
        /// This API is intended to be used only inside postconditions expressed via
        /// <see cref="Odin.DesignContracts.Contract.Ensures(bool,string?,string?)"/>.
        ///
        /// When postconditions are enabled, it is expected that a build-time rewriter will
        /// replace calls to this method with the actual method return value.
        ///
        /// Without rewriting, this method returns <c>default</c>.
        /// </remarks>
        public static T Result<T>()
        {
            return default!;
        }
    }
}