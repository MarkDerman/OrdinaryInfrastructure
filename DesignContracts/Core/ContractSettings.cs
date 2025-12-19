// namespace Odin.DesignContracts
// {
//     /// <summary>
//     /// Represents the configuration for runtime design contract evaluation.
//     /// </summary>
//     /// <remarks>
//     /// Preconditions are always evaluated. This configuration controls the runtime
//     /// evaluation of postconditions and invariants.
//     /// </remarks>
//     public sealed class ContractSettings
//     {
//         /// <summary>
//         /// Gets or sets a value indicating whether postconditions should be evaluated at runtime.
//         /// </summary>
//         /// <remarks>
//         /// When <c>false</c>, calls to <see cref="Contract.Ensures(bool, string?, string?)"/> become no-ops.
//         /// </remarks>
//         public bool EnablePostconditions { get; set; } = true;
//
//         /// <summary>
//         /// Gets or sets a value indicating whether object invariants should be evaluated at runtime.
//         /// </summary>
//         /// <remarks>
//         /// When <c>false</c>, calls to <see cref="Contract.Invariant(bool, string?, string?)"/> become no-ops.
//         /// </remarks>
//         public bool EnableInvariants { get; set; } = true;
//     }
// }