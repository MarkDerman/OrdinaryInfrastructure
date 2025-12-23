namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Full names of all the attributes used in Design Contracts
/// </summary>
internal static class Names
{
    internal const string BclContractNamespace = "System.Diagnostics.Contracts";
    internal const string OdinContractNamespace = "Odin.DesignContracts";
    internal const string OdinPostconditionEnsuresTypeFullName = OdinContractNamespace + ".Contract";
    internal const string OdinInvariantAttributeFullName = OdinContractNamespace + ".ClassInvariantMethodAttribute";
    internal const string BclInvariantAttributeFullName = BclContractNamespace + ".ContractInvariantMethodAttribute";
    internal const string OdinPureAttributeFullName = OdinContractNamespace + ".PureAttribute";
    internal const string BclPureAttributeFullName = BclContractNamespace + ".PureAttribute";
    internal static readonly string[] PureAttributeFullNames = [OdinPureAttributeFullName, BclPureAttributeFullName];
    internal static readonly string[] InvariantAttributeFullNames = [OdinInvariantAttributeFullName, BclInvariantAttributeFullName];
}