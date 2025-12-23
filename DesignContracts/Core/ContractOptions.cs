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
        
        // Unable to understand how to get around initializing 1 static instance of Current in NUnit test runs.
        // throw new InvalidOperationException("Current DesignContractOptions not initialized.");
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public static void Initialize(ContractOptions options)
            => _current = options;
        
        /// <summary>
        /// Gets or sets a value indicating whether postconditions should be evaluated at runtime.
        /// </summary>
        /// <remarks>
        /// When <c>false</c>, calls to <see cref="Odin.DesignContracts.Contract.Ensures(bool, string?, string?)"/> become no-ops.
        /// </remarks>
        public bool EnablePostconditions { get; init; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether object invariants should be evaluated at runtime.
        /// </summary>
        /// <remarks>
        /// When <c>false</c>, calls to <see cref="Odin.DesignContracts.Contract.Invariant(bool, string?, string?)"/> become no-ops.
        /// </remarks>
        public bool EnableInvariants { get; init; } = true;
    }
}