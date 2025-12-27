using Microsoft.Build.Framework;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// MSBuild task for running the Design-by-Contract rewriter.
/// </summary>
public class WeaveDesignContracts : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Runs the Design-by-Contract rewriter on the assembly to rewrite.
    /// </summary>
    /// <returns></returns>
    public override bool Execute()
    {
        var logger = new MsBuildLoggingAdaptor(Log);
        try
        {
            AssemblyRewriter contractsRewriter = new AssemblyRewriter(AssemblyToRewritePath, logger);
            contractsRewriter.Rewrite();
            return true;
        }
        catch (Exception err)
        {
            Log.LogMessage(MessageImportance.High,$"{Names.OdinDesignContractsNamespace}: Unhandled error while rewriting assembly {AssemblyToRewritePath}.");
            Log.LogErrorFromException(err,true,true,AssemblyToRewritePath);
            return false;
        }
       
    }

    /// <summary>
    /// Path to the assembly to be rewritten.
    /// </summary>
    [Required]
    public string AssemblyToRewritePath { get; set; }
}