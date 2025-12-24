

using Odin.DesignContracts;

namespace Targets
{
    public sealed class EnsuresTestTarget
    {
        public int Number { get; set; }
        public int EnsuresPlus5A(int testValue)
        {
            Contract.Ensures(Contract.Result<int>() == testValue + 5);
            if (testValue > 100) return testValue + 2; // Break the post-condition for testValue > 100...
            return testValue + 5;
        }
        
        public int EnsuresPlus5B(int testValue)
        {
            Contract.Ensures(Contract.Result<int>() == testValue + 5);
            Contract.Requires(testValue > 0);
            if (testValue > 100) return testValue + 2; // Break the contract for testValue > 100...
            return testValue + 5;
        }
        
    }
}
