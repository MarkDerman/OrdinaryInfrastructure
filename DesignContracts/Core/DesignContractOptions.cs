namespace Odin.DesignContracts
{
    /// <summary>
    /// Represents the configuration for runtime design contract evaluation.
    /// </summary>
    /// <remarks>
    /// Preconditions are always evaluated. This configuration controls whether runtime
    /// evaluation of postconditions and invariants are skipped or not.
    /// </remarks>
    public sealed class DesignContractOptions
    {
        private static DesignContractOptions? _current;

        /// <summary>
        /// Static facade to the current runtime instance of DesignContractOptions, which must be set
        /// early on in application startup by calling Initialize.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static DesignContractOptions Current =>
            _current ?? throw new InvalidOperationException("Current DesignContractOptions not initialized.");
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public static void Initialize(DesignContractOptions options)
            => _current = options;
        
        /// <summary>
        /// Gets or sets a value indicating whether postconditions should be evaluated at runtime.
        /// </summary>
        /// <remarks>
        /// When <c>false</c>, calls to <see cref="Postcondition.Ensures(bool, string?, string?)"/> become no-ops.
        /// </remarks>
        public bool EnablePostconditions { get; init; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether object invariants should be evaluated at runtime.
        /// </summary>
        /// <remarks>
        /// When <c>false</c>, calls to <see cref="Contract.Invariant(bool, string?, string?)"/> become no-ops.
        /// </remarks>
        public bool EnableInvariants { get; init; } = false;
    }
}