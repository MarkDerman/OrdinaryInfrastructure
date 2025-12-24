namespace Odin.DesignContracts
{
    /// <summary>
    /// Identifies a method that contains class invariant checks for its declaring type.
    /// </summary>
    /// <remarks>
    /// Methods marked with this attribute are expected to be private, parameterless,
    /// and to invoke <see cref="Odin.DesignContracts.Contract.Invariant"/> for each invariant.
    /// Source generators can use this attribute to discover and invoke invariant methods
    /// at appropriate points (for example, at the end of constructors and public methods).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ClassInvariantMethodAttribute : Attribute
    {
    }
    
    // /// <summary>
    // /// Indicates that the decorated method is intended to be used only by
    // /// generated code for contract injection purposes.
    // /// </summary>
    // /// <remarks>
    // /// This attribute is provided primarily as a hint to analyzers and code
    // /// generation tooling. It has no effect at runtime.
    // /// </remarks>
    // [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    // public sealed class ContractGeneratedMethodAttribute : Attribute
    // {
    // }
    
    /// <summary>
    /// Methods and classes marked with this attribute can be used within calls to Contract methods. Such methods do not make any state changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | 
                    AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Class | 
                    AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class PureAttribute : Attribute
    {
    }
}