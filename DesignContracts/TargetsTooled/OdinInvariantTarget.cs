using Odin.DesignContracts;

namespace TargetsTooled
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

        [global::Odin.DesignContracts.ClassInvariantMethod]
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

        [global::Odin.DesignContracts.Pure]
        public int PureGetValue() => _value;

        [global::Odin.DesignContracts.Pure]
        public int PureProperty => _value;

        public int NonPureProperty => _value;
    }
}
