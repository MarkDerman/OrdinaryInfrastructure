using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts.Rewriter;
using Targets;

namespace Tests.Odin.DesignContracts.Rewriter;

[TestFixture]
public sealed class MethodHandlerTests
{
    [Test]
    [TestCase(typeof(OdinInvariantTestTarget),"get_" + nameof(OdinInvariantTestTarget.PureProperty), true)]
    [TestCase(typeof(BclInvariantTestTarget), "get_" + nameof(BclInvariantTestTarget.PureProperty),true)]
    [TestCase(typeof(BclInvariantTestTarget), nameof(BclInvariantTestTarget.PureGetValue),true)]
    [TestCase(typeof(BclInvariantTestTarget), "get_" + nameof(BclInvariantTestTarget.NonPureProperty),false)]
    public void Pure_methods_are_recognised(Type type, string methodName, bool isPure)
    {
        CecilAssemblyContext context = CecilAssemblyContext.GetTargetsUntooledAssemblyContext();
        MethodHandler? sut = GetMethodHandlerFor(context, type, methodName);
            
        Assert.That(sut, Is.Not.Null);
        Assert.That(sut!.IsPure, Is.EqualTo(isPure));
    }

    private MethodHandler? GetMethodHandlerFor(CecilAssemblyContext context, Type type, string methodName)
    {
        TypeDefinition? typeDef = context.FindType(type.FullName!);
        TypeHandler handler = new TypeHandler(typeDef!);
        MethodDefinition? def = handler.Type.Methods.FirstOrDefault(n => n.Name == methodName);
        return new MethodHandler(def!, handler);
    }

}
