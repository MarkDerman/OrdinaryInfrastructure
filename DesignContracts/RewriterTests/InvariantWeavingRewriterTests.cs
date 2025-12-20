using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts;
using Odin.DesignContracts.Rewriter;
using Tests.Odin.DesignContracts.RewriterTargets;

namespace Tests.Odin.DesignContracts.Rewriter;

[TestFixture]
public sealed class InvariantWeavingRewriterTests
{
    [SetUp]
    public void SetUp()
    {
        // Ensure invariants are enabled even if the test environment sets env vars.
        DesignContractOptions.Initialize(new DesignContractOptions
        {
            EnableInvariants = true,
            EnablePostconditions = true
        });
    }

    [Test]
    public void Public_constructor_runs_invariant_on_exit()
    {
        using var context = new RewrittenAssemblyContext(typeof(InvariantTarget).Assembly);
        Assert.That(DesignContractOptions.Current.EnableInvariants, Is.True);
        Assert.That(DesignContractOptions.Current.EnablePostconditions, Is.True);
        
        Type t = context.GetTypeOrThrow(typeof(InvariantTarget).FullName!);

        // 
        ContractException? ex = Assert.Throws<ContractException>(() =>
        {
            try
            {
                Activator.CreateInstance(t, -1);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                throw tie.InnerException;
            }
        });
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test][Description("Increment would only cause an Invariant exception at method entry, as at exit value = 0 would satisfy the invariant.")]
    public void Public_method_runs_invariant_on_entry()
    {
        using var context = new RewrittenAssemblyContext(typeof(InvariantTarget).Assembly);
        Type t = context.GetTypeOrThrow(typeof(InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        ContractException ex = Assert.Throws<ContractException>(() => { Invoke(t, instance, nameof(InvariantTarget.Increment)); })!;

        Assert.That(ex.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test][Description("MakeInvalid would only cause an Invariant exception at method exit, not entry.")]
    public void Public_method_runs_invariant_on_exit()
    {
        using var context = new RewrittenAssemblyContext(typeof(InvariantTarget).Assembly);
        Type t = context.GetTypeOrThrow(typeof(InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;

        ContractException ex = Assert.Throws<ContractException>(() => { Invoke(t, instance, nameof(InvariantTarget.MakeInvalid)); })!;

        Assert.That(ex.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test]
    public void Pure_method_is_excluded_from_invariant_weaving()
    {
        using var context = new RewrittenAssemblyContext(typeof(InvariantTarget).Assembly);
        Type t = context.GetTypeOrThrow(typeof(InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        // If [Pure] is honoured, this returns the invalid value without invariant checks throwing.
        object? result = Invoke(t, instance, nameof(InvariantTarget.PureGetValue));
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Pure_property_is_excluded_from_invariant_weaving()
    {
        using var context = new RewrittenAssemblyContext(typeof(InvariantTarget).Assembly);
        Type t = context.GetTypeOrThrow(typeof(InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        PropertyInfo p = t.GetProperty(nameof(InvariantTarget.PureValue))!;
        object? value = p.GetValue(instance);
        Assert.That(value, Is.EqualTo(-1));
    }

    [Test]
    public void Non_pure_property_is_woven_and_checks_invariants()
    {
        using var context = new RewrittenAssemblyContext(typeof(InvariantTarget).Assembly);
        Type t = context.GetTypeOrThrow(typeof(InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        PropertyInfo p = t.GetProperty(nameof(InvariantTarget.NonPureValue))!;

        ContractException ex = Assert.Throws<ContractException>(() =>
        {
            try
            {
                _ = p.GetValue(instance);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                throw tie.InnerException;
            }
        })!;

        Assert.That(ex.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test]
    public void Multiple_invariant_methods_causes_rewrite_to_fail()
    {
        // Arrange: create a temp copy of this test assembly and inject a second [ClassInvariantMethod].
        string originalPath = typeof(InvariantTarget).Assembly.Location;
        using var temp = new TempDirectory();

        string inputPath = Path.Combine(temp.Path, Path.GetFileName(originalPath));
        File.Copy(originalPath, inputPath, overwrite: true);

        using (AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(inputPath, new ReaderParameters { ReadWrite = false }))
        {
            TypeDefinition targetType = assemblyDefinition.MainModule.GetType(typeof(InvariantTarget).FullName!)
                                        ?? throw new InvalidOperationException("Failed to locate target type in temp assembly.");

            MethodDefinition increment = targetType.Methods
                .First(m => m.Name == nameof(InvariantTarget.Increment));

            // Add a second invariant attribute to a different method.
            var ctor = typeof(ClassInvariantMethodAttribute).GetConstructor(Type.EmptyTypes)
                       ?? throw new InvalidOperationException("Invariant attribute must have a parameterless ctor.");
            var ctorRef = assemblyDefinition.MainModule.ImportReference(ctor);
            increment.CustomAttributes.Add(new CustomAttribute(ctorRef));

            assemblyDefinition.Write(inputPath);
        }

        // Act + Assert
        string outputPath = Path.Combine(temp.Path, "out.dll");
        InvalidOperationException? expectedError = Assert.Throws<InvalidOperationException>(() => RewriterProgram.RewriteAssembly(inputPath, outputPath));
        Assert.That(expectedError, Is.Not.Null);
    }

    private static void SetPrivateField(Type declaringType, object instance, string fieldName, object? value)
    {
        FieldInfo f = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                      ?? throw new InvalidOperationException($"Missing field '{fieldName}'.");
        f.SetValue(instance, value);
    }

    private static object? Invoke(Type declaringType, object instance, string methodName)
    {
        MethodInfo m = declaringType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)
                       ?? throw new InvalidOperationException($"Missing method '{methodName}'.");

        try
        {
            return m.Invoke(instance, null);
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw tie.InnerException;
        }
    }
}

