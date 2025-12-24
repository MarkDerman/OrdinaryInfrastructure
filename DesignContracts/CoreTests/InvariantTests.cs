using Odin.DesignContracts;
using NUnit.Framework;
using TargetsTooled;

namespace Tests.Odin.DesignContracts
{
    [TestFixture]
    public sealed class InvariantTests
    {
        [SetUp]
        public void SetUp()
        {
            ContractOptions.Initialize(new ContractOptions
            {
                Invariants = true,
                Postconditions = true
            });
        }
        
        [Test]
        public void Public_constructor_runs_invariant_on_exit([Values] AttributeFlavour testCase)
        {
            Assert.That(ContractOptions.Current.Invariants, Is.True);
            Assert.That(ContractOptions.Current.Postconditions, Is.True);

            ContractException? ex = Assert.Throws<ContractException>(() =>
            {
                if (testCase == AttributeFlavour.Odin)
                {
                    new OdinInvariantTarget(-1);
                }
                else if (testCase == AttributeFlavour.BaseClassLibrary)
                {
                    new BclInvariantTarget(-1);
                }
            });
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Kind, Is.EqualTo(ContractKind.Invariant));
        }
    }
}