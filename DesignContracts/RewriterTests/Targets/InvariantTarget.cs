using Odin.DesignContracts;

namespace Tests.Odin.DesignContracts.Rewriter.Targets
{
    /// <summary>
    /// Target type used by <see cref="InvariantWeavingRewriterTests"/>. The rewriter is expected to inject
    /// invariant calls at entry/exit of public methods and properties, except where [Pure] is applied.
    /// </summary>
    public sealed class InvariantTarget
    {
        private int _value;

        public InvariantTarget(int value)
        {
            _value = value;
        }

        [ClassInvariantMethod]
        private void ObjectInvariant()
        {
            Postcondition.Invariant(_value >= 0, "_value must be >= 0", "_value >= 0");
        }

        public void Increment()
        {
            _value++;
        }

        public void MakeInvalid()
        {
            _value = -1;
        }

        [Pure]
        public int PureGetValue() => _value;

        [Pure] 
        public int PureValue => _value;

        public int NonPureValue => _value;
    }
}
