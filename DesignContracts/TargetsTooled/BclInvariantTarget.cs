using Odin.DesignContracts;

namespace TargetsTooled
{
    /// <summary>
    /// The rewriter is expected to inject
    /// invariant calls at entry/exit of public methods and properties, except where [Pure] is applied.
    /// </summary>
    public sealed class BclInvariantTarget
    {
        private int _value;

        public BclInvariantTarget(int value)
        {
            _value = value;
        }

        [System.Diagnostics.Contracts.ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_value >= 0, "_value must be non-negative", "_value >= 0");
        }

        public void Increment()
        {
            _value++;
        }

        public void MakeInvalid()
        {
            _value = -1;
        }

        [System.Diagnostics.Contracts.Pure]
        public int PureGetValue() => _value;

        [System.Diagnostics.Contracts.Pure]
        public int PureProperty => _value;
        
        public int NonPureProperty => _value;
    }
}
