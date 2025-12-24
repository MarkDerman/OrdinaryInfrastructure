namespace Odin.DesignContracts
{
    /// <summary>
    /// Represents the configuration for runtime design contract evaluation,
    /// provisionally exposed statically via ContractOptions.Current.
    /// </summary>
    /// <remarks>
    /// Preconditions are always evaluated. This configuration controls whether runtime
    /// evaluation of postconditions, invariants, assertions and assumptions are entirely skipped,
    /// </remarks>
    public sealed record ContractOptions
    {
        /// <summary>
        /// Configures runtime behaviour for individual precondition Requires calls.
        /// </summary>
        public ContractHandlingBehaviour Preconditions { get; init; } = ContractHandlingBehaviour.EventHandlersAndEscalation;
        
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
                if (_current is null)
                {
                    _current = DefaultOn();
                }
                return _current;
            }
        }
        
        /// <summary>
        /// Everything on.
        /// </summary>
        /// <returns></returns>
        public static ContractOptions DefaultOn(ContractHandlingBehaviour behaviour = ContractHandlingBehaviour.EventHandlersAndEscalation)
        {
            return new ContractOptions() { Invariants = behaviour,
                Postconditions = behaviour,
                Assertions = behaviour,
                Assumptions = behaviour
            };
        }
        
        /// <summary>
        /// Preconditions EventHandlersAndEscalation and everything else Bypass.
        /// </summary>
        /// <returns></returns>
        public static ContractOptions DefaultOff()
        {
            return new ContractOptions() { Invariants = ContractHandlingBehaviour.Bypass,  
                Postconditions = ContractHandlingBehaviour.Bypass,
                Assertions = ContractHandlingBehaviour.Bypass,
                Assumptions = ContractHandlingBehaviour.Bypass
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