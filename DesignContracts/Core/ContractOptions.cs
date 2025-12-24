namespace Odin.DesignContracts
{
    /// <summary>
    /// Represents the configuration for runtime design contract evaluation.
    /// </summary>
    /// <remarks>
    /// Preconditions are always evaluated. This configuration controls whether runtime
    /// evaluation of postconditions and invariants are skipped or not.
    /// </remarks>
    public sealed class ContractOptions
    {
        private static ContractOptions? _current;

        /// <summary>
        /// Static facade to the current runtime instance of DesignContractOptions, which must be set
        /// early on in application startup by calling Initialize.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static ContractOptions Current
        {
            get
            {
                // Temporary hack so I can continue with testing the actual rewriting...
                if (_current is null)
                {
                    _current = new ContractOptions() { EnableInvariants = true,  EnablePostconditions = true };
                }
                return _current;
            }
        }
        
        /// <summary>
        /// Unable to understand how to get around initializing 1 static instance of Current in NUnit test runs.
        /// throw new InvalidOperationException("Current DesignContractOptions not initialized.");
        /// </summary>
        /// <param name="options"></param>
        public static void Initialize(ContractOptions options)
            => _current = options;
        
        /// <summary>
        /// Configures whether method Ensures calls weaved into the class by the Contracts Rewriter
        /// before each method return evaluate their conditions or are bypassed.
        /// </summary>
        public bool EnablePostconditions { get; init; } = true;

        /// <summary>
        /// Configures whether calls to the class invariant method weaved into the class
        /// by the Contracts Rewriter are either executed or bypassed.
        /// </summary>
        public bool EnableInvariants { get; init; } = true;

        /// <summary>
        /// Configures whether <see cref="Contract.Assume"/> calls evaluate their conditions or are bypassed.
        /// </summary>
        public bool EnableAssumptions { get; set; } = true;
        
        /// <summary>
        /// Configures whether <see cref="Contract.Assert"/> calls evaluate their conditions or are bypassed.
        /// </summary>
        public bool EnableAssertions { get; set; } = true;
    }
}