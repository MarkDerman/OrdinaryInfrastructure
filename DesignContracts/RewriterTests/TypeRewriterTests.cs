using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts.Rewriter;
using Targets;

namespace Tests.Odin.DesignContracts.Rewriter;

[TestFixture]
public sealed class TypeRewriterTests
{
    [Test]
    [TestCase(typeof(OdinInvariantTestTarget), true)]
    [TestCase(typeof(BclInvariantTestTarget), true)]
    [TestCase(typeof(NoInvariantTestTarget), false)]
    public void Invariant_method_is_found(Type type, bool invariantExpected)
    {
        CecilAssemblyContext context = CecilAssemblyContext.GetTargetsUntooledAssemblyContext();
        TypeDefinition? typeDef = context.FindType(type.FullName!);
        TypeRewriter sut = new TypeRewriter(typeDef!);
        
        Assert.That(sut.HasInvariant, Is.EqualTo(invariantExpected));
        if (invariantExpected) 
            Assert.That(sut.InvariantMethod, Is.Not.Null);
    }

}
