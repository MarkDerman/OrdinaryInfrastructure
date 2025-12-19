namespace Odin.DesignContracts
{

    /// <summary>
    /// Provides methods for runtime validation and enforcement of postconditions, 
    /// ensuring that the supplier class has met their advertised\agreed obligations.
    /// </summary>
    public static class Postcondition
    {
        /// <summary>
        /// Represents the return value of the enclosing method for use within postconditions.
        /// </summary>
        /// <typeparam name="T">The enclosing method return type.</typeparam>
        /// <returns>
        /// The value returned by the enclosing method.
        /// </returns>
        /// <remarks>
        /// This API is intended to be used only inside postconditions expressed via
        /// <see cref="Ensures"/>.
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

        /// <summary>
        /// Marks the end of a contract block at the start of a method.
        /// </summary>
        /// <remarks>
        /// This method exists to support classic Design-by-Contract authoring styles and
        /// build-time rewriting.
        ///
        /// A rewriter may use this as a hint to know where contract declarations end and
        /// normal method logic begins.
        /// </remarks>
        public static void EndContractBlock()
        {
        }

        /// <summary>
        /// Specifies a postcondition that must hold true when the enclosing method returns.
        /// </summary>
        /// <param name="condition">The condition that must be <c>true</c>.</param>
        /// <param name="userMessage">An optional message describing the postcondition.</param>
        /// <param name="conditionText">An optional text representation of the condition expression.</param>
        /// <remarks>
        /// Postconditions are evaluated only when <see cref="ContractRuntime.PostconditionsEnabled"/> is <c>true</c>.
        /// Calls to this method become no-ops when postconditions are disabled.
        /// It is expected that source-generated code will invoke this method at
        /// appropriate points (typically immediately before method exit).
        /// </remarks>
        public static void Ensures(bool condition, string? userMessage = null, string? conditionText = null)
        {
            if (!DesignContractOptions.Current.EnablePostconditions)
            {
                return;
            }

            if (!condition)
            {
                Contract.ReportFailure(
                    ContractFailureKind.Postcondition,
                    userMessage,
                    conditionText);
            }
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