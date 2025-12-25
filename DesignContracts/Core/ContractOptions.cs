namespace Odin.DesignContracts
{
    /// <summary>
    /// Represents the configuration for runtime design contract evaluation,
    /// provisionally exposed statically via ContractOptions.Current.
    /// </summary>
    /// <remarks>
    /// NOTE: Preconditions are NOT always evaluated, they can also be bypassed.
    /// ContractOptions configuration controls the ContractHandlingBehaviour of calls 
    /// to preconditions, postconditions, invariants, assertions, and assumptions.
    /// </remarks>
    public sealed record ContractOptions
    {
        /// <summary>
        /// Configures runtime behaviour for individual precondition Requires calls.
        /// </summary>
        public required ContractHandlingBehaviour Preconditions { get; init; }

        /// <summary>
        /// Configures runtime behaviour for individual postcondition Ensures calls weaved into the class by the Contracts Rewriter
        /// before each method return.
        /// </summary>
        public required ContractHandlingBehaviour Postconditions { get; init; }

        /// <summary>
        /// Configures runtime behaviour for individual Invariant calls in the class invariant method. In future, the intention for
        /// setting to ByPass is that this will also bypass calling of the class invariant method entirely from every weaved invocation.
        /// </summary>
        public required ContractHandlingBehaviour Invariants { get; init; }

        /// <summary>
        /// Configures runtime behaviour for individual <see cref="Contract.Assume"/> calls.
        /// </summary>
        public required ContractHandlingBehaviour Assumptions { get; init; }

        /// <summary>
        /// Configures runtime behaviour for individual <see cref="Contract.Assert"/> calls.
        /// </summary>
        public required ContractHandlingBehaviour Assertions { get; init; }

        /// <summary>
        /// Returns the behaviour for the given contract kind.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ContractHandlingBehaviour GetBehaviourFor(ContractKind kind)
        {
            return kind switch
            {
                ContractKind.Precondition => Preconditions,
                ContractKind.Postcondition => Postconditions,
                ContractKind.Invariant => Invariants,
                ContractKind.Assumption => Assumptions,
                ContractKind.Assertion => Assertions,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown contract kind")
            };
        }

        private static ContractOptions? _current;

        /// <summary>
        /// Static facade to the current runtime instance of DesignContractOptions, which must be set
        /// early on in application startup by calling Initialize.
        /// Defaults to DefaultOn().
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static ContractOptions Current
        {
            get
            {
                // Temporary hack so I can continue with testing the actual rewriting...
                // Todo: Remove and resolve configuration initialization issues.
                if (_current is null)
                {
                    _current = On();
                }

                return _current;
            }
        }

        /// <summary>
        /// Creates options with all Contract operations set to EventHandlersAndEscalation
        /// </summary>
        /// <param name="enableEscalation">Enables escalation of contract violations, normally by throwing ContractExceptions.</param>
        /// <param name="enableEventHandling">Enables event handlers to handle contract violations.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ContractOptions On(bool enableEscalation = true,
            bool enableEventHandling = true)
        {
            if (!enableEscalation && !enableEventHandling)
            {
                throw new ArgumentException(
                    $"{nameof(On)} requires enabling of either {nameof(enableEscalation)} or {nameof(enableEscalation)}.");
            }

            ContractHandlingBehaviour behaviour;
            if (enableEscalation && enableEventHandling)
            {
                behaviour = ContractHandlingBehaviour.EventHandlersAndEscalation;
            }
            else if (enableEscalation)
            {
                behaviour = ContractHandlingBehaviour.EscalationOnly;
            }
            else
            {
                behaviour = ContractHandlingBehaviour.EventHandlersOnly;
            }

            return All(behaviour);
        }


        /// <summary>
        /// Creates options with all Contract operations set the passed behaviour handling.
        /// </summary>
        /// <param name="behaviour">The handling for all Contract operations.</param>
        /// <returns></returns>
        public static ContractOptions All(ContractHandlingBehaviour behaviour)
        {
            return new ContractOptions()
            {
                Invariants = behaviour,
                Postconditions = behaviour,
                Assertions = behaviour,
                Assumptions = behaviour,
                Preconditions = behaviour
            };
        }

        /// <summary>
        /// Creates options with all Contract operations set to Bypass,
        /// EXCLUDING Preconditions which are handled with EventHandlersAndEscalation.
        /// </summary>
        /// <returns></returns>
        public static ContractOptions Off()
        {
            return new ContractOptions()
            {
                Invariants = ContractHandlingBehaviour.Bypass,
                Postconditions = ContractHandlingBehaviour.Bypass,
                Assertions = ContractHandlingBehaviour.Bypass,
                Assumptions = ContractHandlingBehaviour.Bypass,
                Preconditions = ContractHandlingBehaviour.EventHandlersAndEscalation
            };
        }

        /// <summary>
        /// Unable to understand how to get around initializing 1 static instance of Current in NUnit test runs.
        /// throw new InvalidOperationException("Current DesignContractOptions not initialized.");
        /// </summary>
        /// <param name="options"></param>
        public static void Initialize(ContractOptions options)
            => _current = options;
            
    }

    /// <summary>
    /// Configures runtime handling for contract condition evaluation
    /// and violation handling.
    /// </summary>
    public enum ContractHandlingBehaviour : short
    {
        /// <summary>
        /// Skip evaluation of the contract condition entirely,
        /// continuing program execution.
        /// </summary>
        Bypass,

        /// <summary>
        /// Evaluate the contract condition. If it fails,
        /// allow any registered event handlers to handle the failure.
        /// If no event handler marks the ContractFailedEvent as handled,
        /// escalation is triggered, typically as the throwing of a ContractException.
        /// </summary>
        EventHandlersAndEscalation,

        /// <summary>
        /// Evaluate the contract condition. If it fails,
        /// trigger escalation, typically as the throwing of a ContractException.
        /// </summary>
        EscalationOnly,

        /// <summary>
        /// Evaluate the contract condition. If it fails,
        /// allow any registered event handlers to handle the failure.
        /// </summary>
        EventHandlersOnly
    }
}