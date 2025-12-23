using Odin.DesignContracts;

namespace Targets
{
    /// <summary>
    /// The rewriter is expected to inject
    /// invariant calls at entry/exit of public methods and properties, except where [Pure] is applied.
    /// </summary>
    public sealed class NoInvariantTarget
    {
        private int _value;

        public NoInvariantTarget(int value)
        {
            _value = value;
        }
    }
}
