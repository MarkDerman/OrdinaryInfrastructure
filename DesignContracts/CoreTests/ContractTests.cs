using NUnit.Framework;
using Odin.DesignContracts;

namespace Tests.Odin.DesignContracts
{
    [TestFixture]
    public sealed class ContractTests
    {
        [Test]
        public void Contract_failure_handling_depends_on_configured_behaviours(
            [Values] ContractHandlingBehaviour behaviour, [Values] ContractKind kind)
        {
            ContractOptions.Initialize(ContractOptions.All(behaviour));

            AssertConfiguredHandlingFor(kind, behaviour);
            if (behaviour == ContractHandlingBehaviour.Bypass | behaviour == ContractHandlingBehaviour.EventHandlersOnly)
            {
                Assert.DoesNotThrow(() =>
                    Contract.HandleContractCondition(kind, false, "userMessage", "conditionText"));
            }
            else
            {
                ContractException? ex = Assert.Throws<ContractException>(() =>
                    Contract.HandleContractCondition(kind, false, "userMessage", "conditionText"));
                Assert.That(ex, Is.Not.Null);
                Assert.That(ex!.Message, Contains.Substring("userMessage"));
                Assert.That(ex!.Message, Contains.Substring("conditionText"));
            }
        }

        [Test]
        [TestCase(ContractHandlingBehaviour.Bypass, false, false)]
        [TestCase(ContractHandlingBehaviour.EscalationOnly, false, true)]
        [TestCase(ContractHandlingBehaviour.EventHandlersOnly, true, false)]
        [TestCase(ContractHandlingBehaviour.EventHandlersAndEscalation, true, false)]
        public void Contract_handling_fires_ContractFailed_delegates_depending_on_configured_behaviour(
            ContractHandlingBehaviour behaviour, bool handlerShouldFire, bool exceptionShouldBeThrown)
        {
            ContractOptions.Initialize(ContractOptions.All(behaviour));
            foreach (ContractKind kind in AllKinds())
            {
                AssertConfiguredHandlingFor(kind, behaviour);
                bool handlerFiredFlag = false;
                EventHandler<ContractFailedEventArgs> handler = (sender, args) =>
                {
                    handlerFiredFlag = true;
                    args.Handled = true;
                };
                Contract.ContractFailed += handler;

                if (exceptionShouldBeThrown)
                {
                    Assert.Throws<ContractException>(() =>
                        Contract.HandleContractCondition(kind, false, "userMessage", "conditionText"));
                }
                else
                {
                    Contract.HandleContractCondition(kind, false, "userMessage", "conditionText");
                }

                Assert.That(handlerFiredFlag, Is.EqualTo(handlerShouldFire));
                Contract.ContractFailed -= handler;
            }
        }

        [Test]
        [Description("For EventHandlersAndEscalation the event handler can set the Handled property")]
        public void Contract_event_handler_can_handle_contract_failure(
            [Values] ContractKind kind, [Values] bool handleFailure)
        {
            ContractOptions.Initialize(ContractOptions.All(ContractHandlingBehaviour.EventHandlersAndEscalation));
            AssertConfiguredHandlingFor(kind, ContractHandlingBehaviour.EventHandlersAndEscalation);
            bool exceptionShouldBeThrown = !handleFailure;
            EventHandler<ContractFailedEventArgs> handler = (sender, args) =>
            {
                args.Handled = handleFailure;
            };
            Contract.ContractFailed += handler;

            if (exceptionShouldBeThrown)
            {
                Assert.Throws<ContractException>(() =>
                    Contract.HandleContractCondition(kind, false, "msg"));
            }
            else
            {
                Assert.DoesNotThrow(() => 
                    Contract.HandleContractCondition(kind, false, "msg"));
            }
            
            Contract.ContractFailed -= handler;
        }

        [Test]
        [TestCase("userMessage", "conditionText", "{0}: userMessage [Condition: conditionText]")]
        [TestCase("userMessage", "  ", "{0}: userMessage")]
        [TestCase("userMessage", null, "{0}: userMessage")]
        [TestCase("userMessage", "", "{0}: userMessage")]
        [TestCase("", "", "{0}.")]
        [TestCase("", null, "{0}.")]
        [TestCase(" ", null, "{0}.")]
        [TestCase(null, null, "{0}.")]
        [TestCase(null, "", "{0}.")]
        [TestCase(null, " ", "{0}.")]
        [TestCase(null, "conditionText", "{0}: conditionText")]
        [TestCase("", "conditionText", "{0}: conditionText")]
        [TestCase("   ", "conditionText", "{0}: conditionText")]
        public void Escalation_exception_message_formatting(string? userMessage, string? conditionText,
            string messageFormat)
        {
            ContractOptions.Initialize(ContractOptions.On());
            foreach (ContractKind kind in AllKinds())
            {
                string messageExpected = string.Format(messageFormat, Contract.GetKindFailedText(kind));
                ContractException? ex = Assert.Throws<ContractException>(() =>
                    Contract.HandleContractCondition(kind, false, userMessage, conditionText));
                Assert.That(ex, Is.Not.Null);
                Assert.That(ex!.Message, Is.EqualTo(messageExpected), "Exception message is incorrect");
            }
        }

        [Test]
        public void Handle_does_not_escalate_on_success([Values] ContractHandlingBehaviour handlingBehaviour)
        {
            ContractOptions.Initialize(ContractOptions.All(handlingBehaviour));

            AssertConfiguredHandlingFor(ContractKind.Precondition, handlingBehaviour);
            Assert.DoesNotThrow(() => Contract.Requires(true, "Message"));
            Assert.DoesNotThrow(() => Contract.Requires<ArgumentException>(true, "Message"));
            AssertConfiguredHandlingFor(ContractKind.Postcondition, handlingBehaviour);
            Assert.DoesNotThrow(() => Contract.EnsuresImplementation(true, "Message"));
            AssertConfiguredHandlingFor(ContractKind.Invariant, handlingBehaviour);
            Assert.DoesNotThrow(() => Contract.Invariant(true, "Message"));
            AssertConfiguredHandlingFor(ContractKind.Assumption, handlingBehaviour);
            Assert.DoesNotThrow(() => Contract.Assume(true, "Message"));
            AssertConfiguredHandlingFor(ContractKind.Assertion, handlingBehaviour);
            Assert.DoesNotThrow(() => Contract.Assert(true, "Message"));
        }

        [Test]
        public void Requires_not_null_throws_contract_exception_if_argument_null()
        {
            ContractOptions.Initialize(ContractOptions.On());

            ContractException? exception = Assert.Throws<ContractException>(() =>
                Contract.RequiresNotNull(null as string, "myArg is required."));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo("Precondition not met: myArg is required."));
            Assert.That(exception.UserMessage, Is.EqualTo("myArg is required."));
        }

        internal void AssertConfiguredHandlingFor(ContractKind kind, ContractHandlingBehaviour handlingBehaviour)
        {
            Assert.That(ContractOptions.Current.GetBehaviourFor(kind), Is.EqualTo(handlingBehaviour), $"Expected {kind} handling to be {handlingBehaviour}");
        }

        internal ContractKind[] AllKinds()
        {
            return Enum.GetValues<ContractKind>();
        }
    }

    [TestFixture(typeof(ArgumentNullException))]
    [TestFixture(typeof(ArgumentException))]
    [TestFixture(typeof(DivideByZeroException))]
    public sealed class ContractRequiresOfTExceptionTests<TException> where TException : Exception
    {
        [Test]
        public void Requires_of_T_throws_specific_exception()
        {
            TException? ex = Assert.Throws<TException>(() => Contract.Requires<TException>(false, "msg"));

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex, Is.InstanceOf<TException>());
        }
    }
}