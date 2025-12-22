using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Build-time IL rewriter that injects Design-by-Contract postconditions into method exit paths,
/// as well as Design-by-Contract class invariant calls at both entry to and exit from all
/// public members on the API surface, unless marked 'Pure'.
/// </summary>
internal static class Program
{
    private const string Rewriter = "Odin.DesignContracts.Rewriter";

    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine($"{Rewriter}: Usage 'Odin.DesignContracts.Rewriter <assemblyPath> <outputAssemblyPath>'");
            return 3;
        }

        string assemblyPath = args[0];
        string outputPath = args[1];

        if (!File.Exists(assemblyPath))
        {
            Console.Error.WriteLine($"{Rewriter}: Input assembly not found: {assemblyPath}");
            return 2;
        }

        try
        {
            RewriteAssembly(assemblyPath, outputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{Rewriter}: Unexpected error while rewriting assembly...");
            Console.Error.WriteLine($"{Rewriter}: {ex.Message}");
            return 1;
        }
    }

    internal static void RewriteAssembly(string assemblyPath, string outputPath)
    {
        string assemblyDir = Path.GetDirectoryName(Path.GetFullPath(assemblyPath))!;

        DefaultAssemblyResolver resolver = new();
        resolver.AddSearchDirectory(assemblyDir);

        // Portable PDBs are optional. If present, Cecil will pick them up with ReadSymbols = true.
        ReaderParameters readerParameters = new()
        {
            AssemblyResolver = resolver,
            ReadSymbols = File.Exists(Path.ChangeExtension(assemblyPath, ".pdb"))
        };

        using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);

        int rewritten = 0;
        foreach (ModuleDefinition module in assembly.Modules)
        {
            foreach (TypeDefinition type in module.GetTypes())
            {
                TypeHandler currentType = new(type);

                foreach (var member in currentType.GetMembersToTryRewrite())
                {
                    if (!member.TryRewrite())
                        continue;
                    rewritten++;
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);

        WriterParameters writerParameters = new()
        {
            WriteSymbols = readerParameters.ReadSymbols
        };

        assembly.Write(outputPath, writerParameters);
        Console.WriteLine($"Rewriter: rewritten methods: {rewritten}");
    }

   
  
}