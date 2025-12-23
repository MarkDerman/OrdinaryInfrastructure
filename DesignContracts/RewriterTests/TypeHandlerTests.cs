using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts.Rewriter;
using Targets;

namespace Tests.Odin.DesignContracts.Rewriter;

[TestFixture]
public sealed class TypeHandlerTests
{
    [Test]
    [TestCase(typeof(OdinInvariantTarget), true)]
    [TestCase(typeof(BclInvariantTarget), true)]
    [TestCase(typeof(NoInvariantTarget), false)]
    public void Invariant_method_is_found(Type type, bool invariantExpected)
    {
        CecilAssemblyContext context = CecilAssemblyContext.GetTargetsUntooledAssemblyContext();
        TypeDefinition? typeDef = context.FindType(type.FullName!);
        TypeHandler sut = new TypeHandler(typeDef!);
        
        Assert.That(sut.HasInvariant, Is.EqualTo(invariantExpected));
        if (invariantExpected) Assert.That(sut.InvariantMethod, Is.Not.Null);
    }

}
