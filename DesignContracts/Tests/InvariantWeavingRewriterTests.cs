using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using NUnit.Framework;
using Odin.DesignContracts;

namespace Tests.Odin.DesignContracts;

[TestFixture]
public sealed class InvariantWeavingRewriterTests
{
    [SetUp]
    public void SetUp()
    {
        // Ensure invariants are enabled even if the test environment sets env vars.
        ContractRuntime.Configure(new ContractSettings
        {
            EnableInvariants = true,
            EnablePostconditions = true
        });
    }

    [Test]
    public void Public_constructor_runs_invariant_on_exit()
    {
        using var ctx = new RewrittenAssemblyContext(typeof(RewriterTargets.InvariantTarget).Assembly);

        Type t = ctx.GetTypeOrThrow(typeof(RewriterTargets.InvariantTarget).FullName!);

        ContractException ex = Assert.Throws<ContractException>(() =>
        {
            try
            {
                Activator.CreateInstance(t, -1);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                throw tie.InnerException;
            }
        })!;

        Assert.That(ex.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test]
    public void Public_method_runs_invariant_on_entry()
    {
        using var ctx = new RewrittenAssemblyContext(typeof(RewriterTargets.InvariantTarget).Assembly);
        Type t = ctx.GetTypeOrThrow(typeof(RewriterTargets.InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        ContractException ex = Assert.Throws<ContractException>(() =>
        {
            Invoke(t, instance, nameof(RewriterTargets.InvariantTarget.Increment));
        })!;

        Assert.That(ex.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test]
    public void Public_method_runs_invariant_on_exit()
    {
        using var ctx = new RewrittenAssemblyContext(typeof(RewriterTargets.InvariantTarget).Assembly);
        Type t = ctx.GetTypeOrThrow(typeof(RewriterTargets.InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;

        ContractException ex = Assert.Throws<ContractException>(() =>
        {
            Invoke(t, instance, nameof(RewriterTargets.InvariantTarget.MakeInvalid));
        })!;

        Assert.That(ex.Kind, Is.EqualTo(ContractFailureKind.Invariant));
    }

    [Test]
    public void Pure_method_is_excluded_from_invariant_weaving()
    {
        using var ctx = new RewrittenAssemblyContext(typeof(RewriterTargets.InvariantTarget).Assembly);
        Type t = ctx.GetTypeOrThrow(typeof(RewriterTargets.InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        // If [Pure] is honoured, this returns the invalid value without invariant checks throwing.
        object? result = Invoke(t, instance, nameof(RewriterTargets.InvariantTarget.PureGetValue));
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Pure_property_is_excluded_from_invariant_weaving()
    {
        using var ctx = new RewrittenAssemblyContext(typeof(RewriterTargets.InvariantTarget).Assembly);
        Type t = ctx.GetTypeOrThrow(typeof(RewriterTargets.InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        PropertyInfo p = t.GetProperty(nameof(RewriterTargets.InvariantTarget.PureValue))!;
        object? value = p.GetValue(instance);
        Assert.That(value, Is.EqualTo(-1));
    }

    [Test]
    public void Non_pure_property_is_woven_and_checks_invariants()
    {
        using var ctx = new RewrittenAssemblyContext(typeof(RewriterTargets.InvariantTarget).Assembly);
        Type t = ctx.GetTypeOrThrow(typeof(RewriterTargets.InvariantTarget).FullName!);

        object instance = Activator.CreateInstance(t, 1)!;
        SetPrivateField(t, instance, "_value", -1);

        PropertyInfo p = t.GetProperty(nameof(RewriterTargets.InvariantTarget.NonPureValue))!;

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
        // Arrange: create a temp copy of this test assembly and inject a second [ContractInvariantMethod].
        string originalPath = typeof(RewriterTargets.InvariantTarget).Assembly.Location;
        using var temp = new TempDir();

        string inputPath = Path.Combine(temp.Path, Path.GetFileName(originalPath));
        File.Copy(originalPath, inputPath, overwrite: true);

        using (AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(inputPath, new ReaderParameters { ReadWrite = false }))
        {
            TypeDefinition targetType = ad.MainModule.GetType(typeof(RewriterTargets.InvariantTarget).FullName!)
                ?? throw new InvalidOperationException("Failed to locate target type in temp assembly.");

            MethodDefinition increment = targetType.Methods
                .First(m => m.Name == nameof(RewriterTargets.InvariantTarget.Increment));

            // Add a second invariant attribute to a different method.
            var ctor = typeof(Odin.DesignContracts.ContractInvariantMethodAttribute).GetConstructor(Type.EmptyTypes)
                       ?? throw new InvalidOperationException("Invariant attribute must have a parameterless ctor.");
            var ctorRef = ad.MainModule.ImportReference(ctor);
            increment.CustomAttributes.Add(new CustomAttribute(ctorRef));

            ad.Write(inputPath);
        }

        // Act + Assert
        string outputPath = Path.Combine(temp.Path, "out.dll");
        Assert.Throws<InvalidOperationException>(() => Odin.DesignContracts.Rewriter.Program.RewriteAssembly(inputPath, outputPath));
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

    private sealed class RewrittenAssemblyContext : IDisposable
    {
        private readonly AssemblyLoadContext _alc;
        private readonly string _tempDir;

        public Assembly RewrittenAssembly { get; }

        public RewrittenAssemblyContext(Assembly sourceAssembly)
        {
            _tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "odindc-rewriter-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);

            string inputPath = Path.Combine(_tempDir, Path.GetFileName(sourceAssembly.Location));
            File.Copy(sourceAssembly.Location, inputPath, overwrite: true);

            CopyIfExists(Path.ChangeExtension(sourceAssembly.Location, ".pdb"), Path.Combine(_tempDir, Path.GetFileName(Path.ChangeExtension(sourceAssembly.Location, ".pdb"))));
            CopyIfExists(Path.ChangeExtension(sourceAssembly.Location, ".deps.json"), Path.Combine(_tempDir, Path.GetFileName(Path.ChangeExtension(sourceAssembly.Location, ".deps.json"))));

            string outputPath = Path.Combine(_tempDir, "rewritten.dll");
            Odin.DesignContracts.Rewriter.Program.RewriteAssembly(inputPath, outputPath);

            _alc = new TestAssemblyLoadContext(outputPath);
            RewrittenAssembly = _alc.LoadFromAssemblyPath(outputPath);
        }

        public Type GetTypeOrThrow(string fullName)
            => RewrittenAssembly.GetType(fullName, throwOnError: true)!
               ?? throw new InvalidOperationException($"Type not found in rewritten assembly: {fullName}");

        public void Dispose()
        {
            _alc.Unload();

            // Best-effort cleanup. Unload is async-ish; ignore IO failures.
            try { Directory.Delete(_tempDir, recursive: true); } catch { /* ignore */ }
        }

        private static void CopyIfExists(string from, string to)
        {
            if (File.Exists(from))
            {
                File.Copy(from, to, overwrite: true);
            }
        }

        private sealed class TestAssemblyLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public TestAssemblyLoadContext(string mainAssemblyPath)
                : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                string? path = _resolver.ResolveAssemblyToPath(assemblyName);
                if (path is null)
                    return null;

                return LoadFromAssemblyPath(path);
            }
        }
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; }

        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "odindc-rewriter-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { /* ignore */ }
        }
    }
}

namespace Tests.Odin.DesignContracts.RewriterTargets;

/// <summary>
/// Target type used by <see cref="InvariantWeavingRewriterTests"/>. The rewriter is expected to inject
/// invariant calls at entry/exit of public methods and properties, except where [Pure] is applied.
/// </summary>
public sealed class InvariantTarget
{
    private int _value;

    public InvariantTarget(int value)
    {
        _value = value;
    }

    [Odin.DesignContracts.ContractInvariantMethod]
    private void ObjectInvariant()
    {
        Contract.Invariant(_value >= 0, "_value must be >= 0", "_value >= 0");
    }

    public void Increment()
    {
        _value++;
    }

    public void MakeInvalid()
    {
        _value = -1;
    }

    [System.Diagnostics.Contracts.Pure]
    public int PureGetValue() => _value;

    [System.Diagnostics.Contracts.Pure]
    public int PureValue => _value;

    public int NonPureValue => _value;
}
