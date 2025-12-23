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
                EnableInvariants = true,
                EnablePostconditions = true
            });
        }
        
        [Test]
        public void Public_constructor_runs_invariant_on_exit([Values] AttributeFlavour testCase)
        {
            Assert.That(ContractOptions.Current.EnableInvariants, Is.True);
            Assert.That(ContractOptions.Current.EnablePostconditions, Is.True);

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
            Assert.That(ex!.Kind, Is.EqualTo(ContractFailureKind.Invariant));
        }
    }
}