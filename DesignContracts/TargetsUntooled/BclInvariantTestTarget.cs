using Odin.DesignContracts;

namespace Targets
{
    /// <summary>
    /// IMPORTANT: 'CONTRACTS_FULL' needs to be defined as a preprocessor symbol
    /// if one wishes to use the Bcl System.Diagnostics.Contracts attributes...
    /// They are conditional.
    /// </summary>
    public sealed class BclInvariantTestTarget
    {
        private int _value;

        public BclInvariantTestTarget(int value)
        {
            _value = value;
        }

        [System.Diagnostics.Contracts.ContractInvariantMethod]
        public void ObjectInvariant()
        {
            Contract.Invariant(_value >= 0, "_value must be non-negative", "_value >= 0");
        }

        public void Increment()
        {
            _value++;
        }
        
        public async Task<int> AsyncIncrement()
        {
            _value++;
            return await Task.FromResult(_value);
        }

        public void MakeInvalid()
        {
            _value = -1;
        }
        
        public async Task<int> AsyncMakeInvalid()
        {
            _value = -1;
            return await Task.FromResult(_value);
        }

        [System.Diagnostics.Contracts.Pure]
        public void PureCommand()
        {
            
        }
        
        [System.Diagnostics.Contracts.Pure]
        public int PureGetValue() => _value;

        [System.Diagnostics.Contracts.Pure]
        public int PureProperty => _value;
        
        public int NonPureProperty => _value;
        
    }
}
