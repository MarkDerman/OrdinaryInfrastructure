using Odin.DesignContracts;
using NUnit.Framework;

namespace Tests.Odin.DesignContracts
{
    [TestFixture]
    public sealed class ContractTests
    {
        [Test]
        [TestCase("not fred.", "(arg != fred)", "Precondition failed: not fred. [Condition: (arg != fred)]")]
        [TestCase("not fred.", "  ", "Precondition failed: not fred.")]
        [TestCase("not fred.", null, "Precondition failed: not fred.")]
        [TestCase("not fred.", "", "Precondition failed: not fred.")]
        [TestCase("", "", "Precondition failed.")]
        [TestCase("", null, "Precondition failed.")]
        [TestCase(" ", null, "Precondition failed.")]
        [TestCase(null, null, "Precondition failed.")]
        [TestCase(null, "", "Precondition failed.")]
        [TestCase(null, " ", "Precondition failed.")]
        [TestCase(null, "(arg==0)", "Precondition failed: (arg==0)")]
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
    }

    [TestFixture(typeof(ArgumentNullException))]
    [TestFixture(typeof(ArgumentException))]
    [TestFixture(typeof(DivideByZeroException))]
    public sealed class ContractRequiresGenericTests<TException> where TException : Exception
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