using NUnit.Framework;
using Odin.DesignContracts;
using Targets;

namespace Tests.Odin.DesignContracts
{
    [TestFixture][Ignore("Get tooling working first...")]
    public sealed class ContractInvariantTests
    {
        [Test]
        public void Public_constructor_has_invariant_weaved_on_exit([Values] AttributeFlavour testCase)
        {
            Assert.That(ContractOptions.Current.Invariants, 
                Is.EqualTo(ContractHandlingBehaviour.EventHandlersAndEscalation));
            Assert.That(ContractOptions.Current.Postconditions, 
                Is.EqualTo(ContractHandlingBehaviour.EventHandlersAndEscalation));

            ContractException? ex = Assert.Throws<ContractException>(() =>
            {
                if (testCase == AttributeFlavour.Odin)
                {
                    new OdinInvariantTestTarget(-1);
                }
                else if (testCase == AttributeFlavour.BaseClassLibrary)
                {
                    new BclInvariantTestTarget(-1);
                }
            });
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Kind, Is.EqualTo(ContractKind.Invariant));
        }
    }
}