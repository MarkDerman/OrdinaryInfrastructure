namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Full names of all the attributes used in Design Contracts
/// </summary>
internal static class Names
{
    internal const string BclContractNamespace = "System.Diagnostics.Contracts";
    internal const string OdinDesignContractsNamespace = "Odin.DesignContracts";
    internal const string OdinPostconditionEnsuresTypeFullName = OdinDesignContractsNamespace + ".Contract";
    internal const string OdinInvariantAttributeFullName = OdinDesignContractsNamespace + ".ClassInvariantMethodAttribute";
    internal const string BclInvariantAttributeFullName = BclContractNamespace + ".ContractInvariantMethodAttribute";
    internal const string OdinPureAttributeFullName = OdinDesignContractsNamespace + ".PureAttribute";
    internal const string BclPureAttributeFullName = BclContractNamespace + ".PureAttribute";
    internal static readonly string[] PureAttributeFullNames = [OdinPureAttributeFullName, BclPureAttributeFullName];
    internal static readonly string[] InvariantAttributeFullNames = [OdinInvariantAttributeFullName, BclInvariantAttributeFullName];
}