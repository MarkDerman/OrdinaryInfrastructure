

using Odin.DesignContracts;

namespace Targets
{
    public sealed class EnsuresTestTarget
    {
        public int EnsuresPlus5A(int y)
        {
            Contract.Ensures(Contract.Result<int>() == y + 5);
            if (y > 100) return y + 2; // Break the contract for y > 100...
            return y + 5;
        }
        public int EnsuresPlus5B(int y)
        {
            if (y > 100) return y + 2; // Break the contract for y > 100...
            Contract.Ensures(Contract.Result<int>() == y + 5);
            return y + 5;
        }
        
    }
}
