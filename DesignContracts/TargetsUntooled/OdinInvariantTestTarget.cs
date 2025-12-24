using Odin.DesignContracts;

namespace Targets
{
    /// <summary>
    /// The rewriter is expected to inject
    /// invariant calls at entry/exit of public methods and properties, except where [Pure] is applied.
    /// </summary>
    public sealed class OdinInvariantTarget
    {
        private int _value;

        public OdinInvariantTarget(int value)
        {
            _value = value;
        }

        [ClassInvariantMethod]
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

        [Pure]
        public void PureCommand()
        {
            
        }

        
        [Pure]
        public int PureGetValue() => _value;

        [Pure]
        public int PureProperty => _value;

        public int NonPureProperty => _value;

        public void RequiresYGreaterThan10(int y)
        {
            Contract.Requires(y > 10, "y must be greater than 10");
            Console.WriteLine("Instruction 1");
            Console.WriteLine("Instruction 2");
            Console.WriteLine("Instruction 2");
        }
        
        public void EnsuresResultIsZero(int y)
        {
            Contract.Requires(y > 10, "y must be greater than 10");
            Console.WriteLine("Instruction 1");
            Console.WriteLine("Instruction 2");
            Console.WriteLine("Instruction 2");
        }
        
        
    }
}
