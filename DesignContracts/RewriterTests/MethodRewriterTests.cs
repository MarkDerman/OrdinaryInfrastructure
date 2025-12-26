using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts.Rewriter;
using Targets;

namespace Tests.Odin.DesignContracts.Rewriter;

[TestFixture]
public sealed class MethodRewriterTests
{
    [Test]
    [TestCase(typeof(OdinInvariantTestTarget),"get_" + nameof(OdinInvariantTestTarget.PureProperty), true)]
    [TestCase(typeof(BclInvariantTestTarget), "get_" + nameof(BclInvariantTestTarget.PureProperty),true)]
    [TestCase(typeof(BclInvariantTestTarget), nameof(BclInvariantTestTarget.PureGetValue),true)]
    [TestCase(typeof(BclInvariantTestTarget), "get_" + nameof(BclInvariantTestTarget.NonPureProperty),false)]
    public void Pure_methods_are_recognised(Type type, string methodName, bool isPure)
    {
        CecilAssemblyContext context = CecilAssemblyContext.GetTargetsUntooledAssemblyContext();
        MethodRewriter? sut = GetMethodHandlerFor(context, type, methodName);
            
        Assert.That(sut, Is.Not.Null);
        Assert.That(sut!.IsPure, Is.EqualTo(isPure));
    }

    private MethodRewriter? GetMethodHandlerFor(CecilAssemblyContext context, Type type, string methodName)
    {
        TypeDefinition? typeDef = context.FindType(type.FullName!);
        TypeRewriter rewriter = new TypeRewriter(typeDef!);
        MethodDefinition? def = rewriter.Type.Methods.FirstOrDefault(n => n.Name == methodName);
        return new MethodRewriter(def!, rewriter);
    }

}
