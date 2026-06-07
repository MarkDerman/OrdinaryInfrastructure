using NUnit.Framework;
using Odin.DesignContracts;

namespace Tests.Odin.DesignContracts;

[TestFixture]
public class ContractOptionsTests
{
    [Test]
    [TestCase(true,true, false, ContractHandlingBehaviour.EventHandlersAndEscalation)]
    [TestCase(true,false, false, ContractHandlingBehaviour.EscalationOnly)]
    [TestCase(false,true, false, ContractHandlingBehaviour.EventHandlersOnly)]
    [TestCase(false,false, true, null)]
    public void On(bool enableEscalation, bool enableEventHandlers,
        bool invalidCombination, ContractHandlingBehaviour? expectedHandling)
    {
        if (invalidCombination)
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                ContractOptions.On(enableEscalation, enableEventHandlers));
            Assert.That(exception.Message, Is.Not.Null);
            return;
        }

        var sut = ContractOptions.On(enableEscalation,enableEventHandlers);
        
        Assert.That(sut.Preconditions, Is.EqualTo(expectedHandling));
        Assert.That(sut.Postconditions, Is.EqualTo(expectedHandling));
        Assert.That(sut.Assertions, Is.EqualTo(expectedHandling));
        Assert.That(sut.Assumptions, Is.EqualTo(expectedHandling));
        Assert.That(sut.Invariants, Is.EqualTo(expectedHandling));
    }
    
    [Test]
    public void Off_sets_to_Bypass()
    {
        ContractOptions sut = ContractOptions.Off();
        
        Assert.That(sut.Preconditions, Is.EqualTo(ContractHandlingBehaviour.EventHandlersAndEscalation));
        Assert.That(sut.Postconditions, Is.EqualTo(ContractHandlingBehaviour.Bypass));
        Assert.That(sut.Assertions, Is.EqualTo(ContractHandlingBehaviour.Bypass));
        Assert.That(sut.Assumptions, Is.EqualTo(ContractHandlingBehaviour.Bypass));
        Assert.That(sut.Invariants, Is.EqualTo(ContractHandlingBehaviour.Bypass));
    }
    
    [Test]
    public void All_sets_everything([Values] ContractHandlingBehaviour handling)
    {
        var sut = ContractOptions.All(handling);
        
        Assert.That(sut.Preconditions, Is.EqualTo(handling));
        Assert.That(sut.Postconditions, Is.EqualTo(handling));
        Assert.That(sut.Assertions, Is.EqualTo(handling));
        Assert.That(sut.Assumptions, Is.EqualTo(handling));
        Assert.That(sut.Invariants, Is.EqualTo(handling));
    }
}