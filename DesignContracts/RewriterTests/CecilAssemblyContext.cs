using System.Reflection;
using Mono.Cecil;
using Targets;

namespace Tests.Odin.DesignContracts.Rewriter;

/// <summary>
/// Encapsulates access to a Cecil Assembly context for a given assembly for testing...
/// </summary>
internal sealed class CecilAssemblyContext : IDisposable
{
    public AssemblyDefinition Assembly { get; }
    
    public static CecilAssemblyContext GetTargetsUntooledAssemblyContext()
    {
        return new CecilAssemblyContext(typeof(OdinInvariantTestTarget).Assembly);
    }
    
    public CecilAssemblyContext(Assembly sourceAssembly)
    {
        string assemblyPath = sourceAssembly.Location;
        string assemblyDir = Path.GetDirectoryName(Path.GetFullPath(sourceAssembly.Location))!;
        DefaultAssemblyResolver resolver = new();
        resolver.AddSearchDirectory(assemblyDir);
        ReaderParameters readerParameters = new()
        {
            AssemblyResolver = resolver,
            ReadSymbols = File.Exists(Path.ChangeExtension(assemblyPath, ".pdb")),
        };

        Assembly = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
    }

    public TypeDefinition? FindType(string fullName)
    {
        return AllTypes.FirstOrDefault(t => t.FullName == fullName);
    }

    public TypeDefinition? FindType(string nameSpace, string typeName)
    {
        return FindType($"{nameSpace}.{typeName}");
    }

    private List<TypeDefinition>? _allTypes;
    public IReadOnlyList<TypeDefinition> AllTypes
    {
        get
        {
            if (_allTypes == null)
            {
                _allTypes = Assembly.Modules.SelectMany(m => m.GetTypes()).ToList();
            }
            return _allTypes;
        }
    }

    public void Dispose()
    {
        Assembly.Dispose();
    }
    
}