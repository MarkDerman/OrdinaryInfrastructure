using Microsoft.Build.Framework;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Build-time IL rewriter that injects Design-by-Contract postconditions into method exit paths,
/// as well as Design-by-Contract class invariant calls at both entry to and exit from all
/// public members on the API surface, unless marked 'Pure'.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine($"{Names.OdinDesignContractsRewriter}: Usage 'dotnet {Names.OdinDesignContractsRewriter}.dll <assemblyPath> <optional:outputAssemblyPath>'");
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
            Console.Error.WriteLine($"{Names.OdinDesignContractsRewriter}: Input assembly not found: {assemblyPath}");
            return 2;
        }

        var logger = new ConsoleLoggingAdaptor();
        try
        {
            AssemblyRewriter contractsRewriter = new AssemblyRewriter(assemblyPath, logger,outputPath);
            contractsRewriter.Rewrite();
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogMessage(LogImportance.High,$"{Names.OdinDesignContractsRewriter}: Unexpected error while rewriting assembly..." );
            logger.LogErrorFromException(ex, true, true,assemblyPath);
            return 1;
        }
    }



   
  
}