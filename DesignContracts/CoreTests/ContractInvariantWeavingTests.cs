using NUnit.Framework;
using Odin.DesignContracts;
using Targets;

namespace Tests.Odin.DesignContracts
{
    [TestFixture]
    public sealed class ContractInvariantWeavingTests
    {
        [SetUp]
        public void Setup()
        {
            ContractOptions.Initialize(ContractOptions.All(ContractHandlingBehaviour.EscalationOnly));
        }
        
        [Test]
        public void Public_constructor_has_invariant_woven_on_exit([Values] AttributeFlavour testCase)
        {
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
            AssertTestInvariantExceptionThrown(ex);
        }
        
        [Test]
        public void Public_method_has_invariant_call_woven_on_entry([Values] AttributeFlavour testCase)
        {
            Type testTypeFor = TestSupport.GetTargetTestTypeFor(testCase);

            object instance = Activator.CreateInstance(testTypeFor, 1)!;
            TestSupport.SetPrivateField(testTypeFor, instance, "_value", -1);

            Exception? ex = Assert.Catch(() =>
            {
                TestSupport.Invoke(testTypeFor, instance, nameof(OdinInvariantTestTarget.Increment));
            })!;
            AssertTestInvariantExceptionThrown(ex);
        }
        
        private void AssertTestInvariantExceptionThrown(Exception? ex)
        {
            Assert.That(ex, Is.Not.Null);
            ContractException? contractException = ex as ContractException;
            Assert.That(contractException, Is.Not.Null);
            Assert.That(contractException.Kind, Is.EqualTo(ContractKind.Invariant));
            Assert.That(contractException.UserMessage, Is.EqualTo("value must be non-negative"));
            Assert.That(contractException.ConditionText, Is.Null);
        }
    }
}