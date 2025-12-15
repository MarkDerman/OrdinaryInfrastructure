
namespace Odin.DesignContracts
{
    /// <summary>
    /// Defines the semantic category of a design contract failure.
    /// </summary>
    public enum ContractFailureKind : short
    {
        /// <summary>
        /// A precondition was broken.
        /// </summary>
        Precondition = 0,

        /// <summary>
        /// A postcondition was not satisfied.
        /// </summary>
        Postcondition = 1,

        /// <summary>
        /// A class invariant check failed.
        /// </summary>
        Invariant = 2,

        /// <summary>
        /// An assertion did not hold.
        /// </summary>
        Assertion = 3,

        /// <summary>
        /// An assumption did not hold.
        /// </summary>
        Assumption = 4
    }
}