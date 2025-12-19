using Odin.DesignContracts;
using NUnit.Framework;

namespace Tests.Odin.DesignContracts
{
    [TestFixture]
    public sealed class PreconditionTests
    {
        [Test]
        [TestCase("not fred.", "(arg != fred)", "Precondition not met: not fred. [Condition: (arg != fred)]")]
        [TestCase("not fred.", "  ", "Precondition not met: not fred.")]
        [TestCase("not fred.", null, "Precondition not met: not fred.")]
        [TestCase("not fred.", "", "Precondition not met: not fred.")]
        [TestCase("", "", "Precondition not met.")]
        [TestCase("", null, "Precondition not met.")]
        [TestCase(" ", null, "Precondition not met.")]
        [TestCase(null, null, "Precondition not met.")]
        [TestCase(null, "", "Precondition not met.")]
        [TestCase(null, " ", "Precondition not met.")]
        [TestCase(null, "(arg==0)", "Precondition not met: (arg==0)")]
        public void Requires_throws_exception_with_correct_message_on_precondition_failure(string conditionDescription, string? conditionText, string expectedExceptionMessage)
        {
            ContractException? ex = Assert.Throws<ContractException>(() => Precondition.Requires(false, conditionDescription,conditionText));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Is.EqualTo(expectedExceptionMessage), "Exception message is incorrect");
        }

        [Test]
        public void Requires_does_not_throw_exception_on_precondition_success()
        {
            Assert.DoesNotThrow(() => Precondition.Requires(true, "Message"), "Precondition success must not throw an Exception");
        }
        
        [Test]
        public void Requires_not_null_throws_contract_exception_if_argument_null()
        {
            ContractException? exception = Assert.Throws<ContractException>(() => 
                Precondition.RequiresNotNull(null as string, "myArg is required."));
            
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo("Precondition not met: myArg is required."));
            Assert.That(exception.UserMessage, Is.EqualTo("myArg is required."));
        }
    }

    [TestFixture(typeof(ArgumentNullException))]
    [TestFixture(typeof(ArgumentException))]
    [TestFixture(typeof(DivideByZeroException))]
    public sealed class PreconditionGenericTests<TException> where TException : Exception
    {
        [Test]
        public void Requires_throws_specific_exception_on_precondition_failure()
        {
            TException? ex = Assert.Throws<TException>(() => Precondition.Requires<TException>(false, "msg"));

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex, Is.InstanceOf<TException>());
        }
    }
}