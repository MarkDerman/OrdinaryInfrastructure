using System.Reflection;
using System.Runtime.Loader;
using Odin.DesignContracts;
using Odin.DesignContracts.Rewriter;

namespace Tests.Odin.DesignContracts.Rewriter;

internal sealed class RewrittenAssemblyContext : IDisposable
{
    private readonly AssemblyLoadContext _alc;
    private readonly string _tempDir;

    public Assembly RewrittenAssembly { get; }

    public RewrittenAssemblyContext(Assembly sourceAssembly, DesignContractOptions? initializeOptions = null)
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rewrite", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        string inputPath = Path.Combine(_tempDir, Path.GetFileName(sourceAssembly.Location));
        File.Copy(sourceAssembly.Location, inputPath, overwrite: true);

        CopyIfExists(Path.ChangeExtension(sourceAssembly.Location, ".pdb"), Path.Combine(_tempDir, Path.GetFileName(Path.ChangeExtension(sourceAssembly.Location, ".pdb"))));
        CopyIfExists(Path.ChangeExtension(sourceAssembly.Location, ".deps.json"), Path.Combine(_tempDir, Path.GetFileName(Path.ChangeExtension(sourceAssembly.Location, ".deps.json"))));

        string outputPath = Path.Combine(_tempDir, "rewritten.dll");
        RewriterProgram.RewriteAssembly(inputPath, outputPath);

        _alc = new TestAssemblyLoadContext(outputPath);
        RewrittenAssembly = _alc.LoadFromAssemblyPath(outputPath);

        // if (initializeOptions == null) return;
        // GetTypeOrThrow("Odin.DesignContracts.")
    }

    public Type GetTypeOrThrow(string fullName)
        => RewrittenAssembly.GetType(fullName, throwOnError: true)!
           ?? throw new InvalidOperationException($"Type not found in rewritten assembly: {fullName}");

    public void Dispose()
    {
        _alc.Unload();

        // Best-effort cleanup. Unload is async-ish; ignore IO failures.
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            /* ignore */
        }
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