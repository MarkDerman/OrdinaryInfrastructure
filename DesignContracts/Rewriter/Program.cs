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
        if (args.Length < 1)
        {
            Console.Error.WriteLine($"{Rewriter}: Usage 'dotnet {Rewriter}.dll <assemblyPath> <optional:outputAssemblyPath>'");
            return 3;
        }

        string assemblyPath = args[0];
        string? outputPath = null;
        if (args.Length >= 2)
        {
            outputPath = args[1];
        }

        if (!File.Exists(assemblyPath))
        {
            Console.Error.WriteLine($"{Rewriter}: Input assembly not found: {assemblyPath}");
            return 2;
        }

        try
        {
            AssemblyRewriter contractsRewriter = new AssemblyRewriter(assemblyPath, outputPath);
            contractsRewriter.Rewrite();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{Rewriter}: Unexpected error while rewriting assembly...");
            Console.Error.WriteLine($"{Rewriter}: {ex.Message}");
            return 1;
        }
    }



   
  
}