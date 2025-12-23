using System.Diagnostics.Contracts;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts;
using Odin.DesignContracts.Rewriter;
using Targets;

namespace Tests.Odin.DesignContracts.Rewriter;

[TestFixture]
public sealed class RewriterTests
{
    [SetUp]
    public void SetUp()
    {
        // Ensure invariants are enabled even if the test environment sets env vars.
        ContractOptions.Initialize(new ContractOptions
        {
            EnableInvariants = true,
            EnablePostconditions = true
        });
    }

    [Test]
    public void Public_constructor_runs_invariant_on_exit([Values] AttributeFlavour testCase)
    {
        Type targetType = GetTargetTestTypeFor(testCase);
        using RewrittenAssemblyContext context = new RewrittenAssemblyContext(targetType.Assembly);
        Assert.That(ContractOptions.Current.EnableInvariants, Is.True);
        Assert.That(ContractOptions.Current.EnablePostconditions, Is.True);

        Type t = context.GetTypeOrThrow(targetType.FullName!);

        // 
        Exception? exception = Assert.Catch(() =>
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
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo("Invariant broken: _value must be non-negative [Condition: _value >= 0]"));
        // Because the ContractException thrown is from the dynamically loaded RewrittenAssemblyContext
        // it does not seem castable to ContractException....
        // Assert.That(exception, Is.InstanceOf<ContractException>());
        // ContractException ex = (ContractException)exception!;
        // Assert.That(ex!.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test]
    [Description("Increment would only cause an Invariant exception at method entry, as at exit value = 0 would satisfy the invariant.")]
    public void Public_method_runs_invariant_on_entry([Values] AttributeFlavour testCase)
    {
        Type targetType = GetTargetTestTypeFor(testCase);
        using RewrittenAssemblyContext context = new RewrittenAssemblyContext(targetType.Assembly);
        Type t = context.GetTypeOrThrow(targetType.FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        Exception? ex = Assert.Catch(() => { Invoke(t, instance, nameof(OdinInvariantTarget.Increment)); })!;

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Invariant broken:"));
    }

    [Test]
    [Description("MakeInvalid would only cause an Invariant exception at method exit, not entry.")]
    public void Public_method_runs_invariant_on_exit([Values] AttributeFlavour testCase)
    {
        Type targetType = GetTargetTestTypeFor(testCase);
        using RewrittenAssemblyContext context = new RewrittenAssemblyContext(targetType.Assembly);
        Type t = context.GetTypeOrThrow(targetType.FullName!);

        object instance = Activator.CreateInstance(t, 1)!;

        Exception? ex = Assert.Catch(() => { Invoke(t, instance, nameof(OdinInvariantTarget.MakeInvalid)); })!;

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Invariant broken:"));
    }

    [Test]
    public void Pure_method_is_excluded_from_invariant_weaving([Values] AttributeFlavour testCase)
    {
        Type targetType = GetTargetTestTypeFor(testCase);
        using RewrittenAssemblyContext context = new RewrittenAssemblyContext(targetType.Assembly);
        Type t = context.GetTypeOrThrow(targetType.FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        // If [Pure] is honoured, this returns the invalid value without invariant checks throwing.
        object? result = Invoke(t, instance, "PureGetValue");

        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Pure_property_is_excluded_from_invariant_weaving([Values] AttributeFlavour testCase)
    {
        Type targetType = GetTargetTestTypeFor(testCase);
        using RewrittenAssemblyContext context = new RewrittenAssemblyContext(targetType.Assembly);
        Type t = context.GetTypeOrThrow(targetType.FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        object? value = Invoke(t, instance, "get_PureProperty");

        Assert.That(value, Is.EqualTo(-1));
    }

    [Test]
    public void Non_pure_property_is_woven_and_checks_invariants([Values] AttributeFlavour testCase)
    {
        Type targetType = GetTargetTestTypeFor(testCase);
        using RewrittenAssemblyContext context = new RewrittenAssemblyContext(targetType.Assembly);
        Type t = context.GetTypeOrThrow(targetType.FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        PropertyInfo p = t.GetProperty("NonPureProperty")!;

        Exception? ex = Assert.Catch(() =>
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
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Invariant broken:"));
    }

    [Test]
    public void Multiple_invariant_methods_causes_rewrite_to_fail([Values] AttributeFlavour firstInvariantFlavour, 
        [Values] AttributeFlavour secondInvariantFlavour)
    {
        // Arrange: create a temp copy of this test assembly and inject a second [ClassInvariantMethod].
        Type firstType = GetTargetTestTypeFor(firstInvariantFlavour);
        Type invariantAttributeTestCaseType = GetClassInvariantAttributeTypeFor(firstInvariantFlavour);
        
        string originalPath = firstType.Assembly.Location;
        using TempDirectory temp = new TempDirectory();

        string inputPath = Path.Combine(temp.Path, Path.GetFileName(originalPath));
        File.Copy(originalPath, inputPath, overwrite: true);

        // Read the bytes into memory first to fully decouple the reader from the file on disk.
        byte[] assemblyBytes = File.ReadAllBytes(inputPath);
        using (MemoryStream ms = new MemoryStream(assemblyBytes))
        using (AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, 
                   new ReaderParameters { ReadWrite = false, ReadingMode = ReadingMode.Immediate }))
        {
            TypeDefinition targetType = assemblyDefinition.MainModule.GetType(firstType.FullName!)
                                        ?? throw new InvalidOperationException("Failed to locate target type in temp assembly.");

            MethodDefinition increment = targetType.Methods
                .First(m => m.Name == nameof(OdinInvariantTarget.Increment));

            // Add a second invariant attribute to a different method.
            ConstructorInfo ctor = invariantAttributeTestCaseType.GetConstructor(Type.EmptyTypes)
                                   ?? throw new InvalidOperationException("Invariant attribute must have a parameterless ctor.");
            MethodReference? ctorRef = assemblyDefinition.MainModule.ImportReference(ctor);
            increment.CustomAttributes.Add(new CustomAttribute(ctorRef));

            assemblyDefinition.Write(inputPath);
        }

        // Act + Assert
        string outputPath = Path.Combine(temp.Path, "out.dll");
        InvalidOperationException? expectedError = Assert.Throws<InvalidOperationException>(() => Program.RewriteAssembly(inputPath, outputPath));
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
    
    private Type GetTargetTestTypeFor(AttributeFlavour testCase)
    {
        if (testCase == AttributeFlavour.Odin)
        {
            return typeof(OdinInvariantTarget);
        }
        if (testCase == AttributeFlavour.BaseClassLibrary)
        {
            return typeof(BclInvariantTarget);
        }
        throw new NotSupportedException(testCase.ToString());
    }
    
    private Type GetClassInvariantAttributeTypeFor(AttributeFlavour testCase)
    {
        if (testCase == AttributeFlavour.Odin)
        {
            return typeof(ClassInvariantMethodAttribute);
        }
        if (testCase == AttributeFlavour.BaseClassLibrary)
        {
            return typeof(ContractInvariantMethodAttribute);
        }
        throw new NotSupportedException(testCase.ToString());
    }
}