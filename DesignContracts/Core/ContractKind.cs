
namespace Odin.DesignContracts
{
    /// <summary>
    /// The different design contract contexts.
    /// </summary>
    public enum ContractKind : short
    {
        /// <summary>
        /// A requirement on a class member that the client caller is
        /// expected to honour.
        /// </summary>
        Precondition = 0,

        /// <summary>
        /// A requirement on a class member the member itself is expected to honour
        /// when returning from the call.
        /// </summary>
        Postcondition = 1,

        /// <summary>
        /// An assertion of a condition that is expected to be true for the class
        /// in question to be in a valid state.
        /// </summary>
        Invariant = 2,

        /// <summary>
        /// A condition that is simply expected to be true at any point in program execution.
        /// </summary>
        Assertion = 3,

        /// <summary>
        /// An assumption is stated primarily as a hint to static analysis tools
        /// about a condition that the tool cannot deduce on its own. Assumptions
        /// are evaluated at runtime the same as Assertions.
        /// </summary>
        Assumption = 4
    }
}