using Microsoft.Build.Framework;
using Mono.Cecil;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Handles Design-by-Contract rewriting of a given assembly.
/// </summary>
internal class AssemblyRewriter
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="targetAssemblyPath">The input assembly path,
    /// typically in the 'intermediate build output' under the 'obj' folder.</param>
    /// <param name="logger"></param>
    /// <param name="alternativeOutputPath">If omitted, defaults to 'InputAssembly.odin-design-contracts-weaved.dll'
    /// in the same folder as the input assembly.</param>
    /// <exception cref="FileNotFoundException">Thrown if the assembly to rewrite does not exist.</exception>
    internal AssemblyRewriter(string targetAssemblyPath, ILoggingAdaptor logger,
        string? alternativeOutputPath = null)
    {
        TargetAssemblyPath = targetAssemblyPath;
        _logger = logger;
        AlternativeOutputPath = alternativeOutputPath;

        if (!File.Exists(TargetAssemblyPath))
        {
            var msg = $"Assembly not found: {TargetAssemblyPath}";
            _logger.LogMessage(LogImportance.High,msg);
            throw new FileNotFoundException(msg);
        }
    }

    private readonly ILoggingAdaptor _logger;
    
    /// <summary>
    /// The assembly to rewrite. Note that if OutputPath is specified,
    /// the rewritten assembly is saved to OutputPath, else the assembly located
    /// as TargetPath is overwritten after a call to Rewrite()
    /// </summary>
    internal string TargetAssemblyPath { get; }
    
    internal string TargetAssemblyPdbPath 
        => Path.ChangeExtension(TargetAssemblyPath, ".pdb"); 

    /// <summary>
    /// Optional alternative path that the rewritten assembly should be written to,
    /// else TargetAssemblyPath is overwritten by default.
    /// </summary>
    internal string? AlternativeOutputPath { get; }
    
    private string GetOutputPath()
        => AlternativeOutputPath ?? TargetAssemblyPath;
    
    /// <summary>
    /// Writes invariants and postconditions into the assembly at TargetPath,
    /// or OutputPath if specified.
    /// </summary>
    internal void Rewrite()
    {
        string assemblyDir = Path.GetDirectoryName(Path.GetFullPath(TargetAssemblyPath))!;
        DefaultAssemblyResolver resolver = new();
        resolver.AddSearchDirectory(assemblyDir);

        // Debug symbols in a .pdb file may or may not be present.
        // We'll read them via a stream if present to ensure no issues when
        ReaderParameters readerParameters;
        bool symbolsPresent = File.Exists(TargetAssemblyPdbPath);
        if (symbolsPresent)
        { 
            MemoryStream symbolStream = new MemoryStream(File.ReadAllBytes(TargetAssemblyPdbPath));
            symbolStream.Seek(0, SeekOrigin.Begin);
            readerParameters = new()
            {
                AssemblyResolver = resolver,
                ReadSymbols = true,
                SymbolStream = symbolStream
            };
        }
        else
        {
            readerParameters = new()
            {
                AssemblyResolver = resolver,
                ReadSymbols = false
            };
        }

        // Read the bytes into memory first to fully decouple the reader from the file on disk.
        using MemoryStream ms = new MemoryStream(File.ReadAllBytes(TargetAssemblyPath));
        using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(ms, readerParameters);
        int rewritten = 0;
        foreach (ModuleDefinition module in assembly.Modules)
        {
            foreach (TypeDefinition type in module.GetTypes())
            {
                TypeRewriter currentType = new(type, _logger);
                rewritten += currentType.Rewrite();
            }
        }
        WriterParameters writerParameters = new()
        {
            WriteSymbols = symbolsPresent
        };
        assembly.Write(GetOutputPath(), writerParameters);
    }
}