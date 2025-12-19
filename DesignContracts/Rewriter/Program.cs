using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Build-time IL rewriter that injects Design-by-Contract postconditions into method exit paths,
/// as well as Design-by-Contract class invariant calls at both entry to and exit from all
/// public members on the API surface, unless marked 'Pure'.
/// </summary>
internal static class RewriterProgram
{
    private const string ContractTypeFullName = "Odin.DesignContracts.Contract";

    private const string OdinInvariantAttributeFullName = "Odin.DesignContracts.ClassInvariantMethodAttribute";
    private const string BclInvariantAttributeFullName  = "System.Diagnostics.Contracts.ClassInvariantMethodAttribute";
    private const string PureAttributeFullName          = "System.Diagnostics.Contracts.PureAttribute";

    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: Odin.DesignContracts.Rewriter <assemblyPath> <outputAssemblyPath>");
            return 2;
        }

        string assemblyPath = args[0];
        string outputPath = args[1];

        if (!File.Exists(assemblyPath))
        {
            Console.Error.WriteLine($"Assembly not found: {assemblyPath}");
            return 2;
        }

        try
        {
            RewriteAssembly(assemblyPath, outputPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    internal static void RewriteAssembly(string assemblyPath, string outputPath)
    {
        string assemblyDir = Path.GetDirectoryName(Path.GetFullPath(assemblyPath))!;

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(assemblyDir);

        // Portable PDBs are optional. If present, Cecil will pick them up with ReadSymbols = true.
        var readerParameters = new ReaderParameters
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
                MethodDefinition? invariantMethod = FindInvariantMethodOrThrow(type);

                foreach (MethodDefinition method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    if (!TryRewriteMethod(type, method, invariantMethod))
                        continue;

                    rewritten++;
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);

        var writerParameters = new WriterParameters
        {
            WriteSymbols = readerParameters.ReadSymbols
        };

        assembly.Write(outputPath, writerParameters);
        Console.WriteLine($"Rewriter: rewritten methods: {rewritten}");
    }

    private static bool TryRewriteMethod(TypeDefinition declaringType, MethodDefinition method, MethodDefinition? invariantMethod)
    {
        // Only handle sync (v1). We rely on analyzers to enforce this, but be defensive.
        // if (method.IsAsync)
        //     return false;

        method.Body.SimplifyMacros();

        bool hasInvariant = invariantMethod is not null;
        bool canWeaveInvariant = hasInvariant && IsPublicInstanceMethodOrAccessor(method);
        bool isInvariantMethodItself = invariantMethod is not null && method == invariantMethod;

        // Per requirements:
        // - Constructors: invariants at exit only
        // - Other public instance methods + public instance property accessors: invariants at entry and exit
        bool isConstructor = method.IsConstructor && !method.IsStatic;

        bool weaveInvariantOnEntry = canWeaveInvariant && !isInvariantMethodItself && !isConstructor;
        bool weaveInvariantOnExit  = canWeaveInvariant && !isInvariantMethodItself;

        // Exclude [Pure] methods and [Pure] properties (for accessors).
        if (!isConstructor && weaveInvariantOnEntry && IsPure(declaringType, method))
        {
            weaveInvariantOnEntry = false;
            weaveInvariantOnExit  = false;
        }

        // If we have no invariant weaving and no postconditions, skip quickly.
        if (!weaveInvariantOnEntry && !weaveInvariantOnExit)
        {
            // Still allow postconditions rewriting.
        }

        bool hasContractBlock = TryExtractContractBlock(method, out List<Instruction> contractBlockInstructions);
        bool hasEnsures = hasContractBlock && contractBlockInstructions.Any(IsEnsuresCall);

        if (!hasEnsures && !weaveInvariantOnEntry && !weaveInvariantOnExit)
        {
            method.Body.OptimizeMacros();
            return false;
        }

        var il = method.Body.GetILProcessor();

        // Inject invariant call at entry (before any user code).
        if (weaveInvariantOnEntry)
        {
            Instruction first = method.Body.Instructions.FirstOrDefault() ?? Instruction.Create(OpCodes.Nop);
            if (method.Body.Instructions.Count == 0)
                il.Append(first);

            InsertInvariantCallBefore(il, first, invariantMethod!);
        }

        // Remove the contract block from the method entry when postconditions are present.
        if (hasEnsures)
        {
            foreach (Instruction inst in contractBlockInstructions)
            {
                // If the instruction was already removed as part of a previous remove, skip.
                if (method.Body.Instructions.Contains(inst))
                    il.Remove(inst);
            }
        }

        // If we need to inject postconditions and/or invariant calls at exit, do it per-ret.
        if (hasEnsures || weaveInvariantOnExit)
        {
            bool isVoid = method.ReturnType.MetadataType == MetadataType.Void;
            VariableDefinition? resultVar = null;

            if (!isVoid)
            {
                resultVar = new VariableDefinition(method.ReturnType);
                method.Body.Variables.Add(resultVar);
                method.Body.InitLocals = true;
            }

            List<Instruction> rets = method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
            foreach (Instruction ret in rets)
            {
                // For non-void methods we must preserve the return value while we call extra code.
                if (!isVoid)
                {
                    il.InsertBefore(ret, Instruction.Create(OpCodes.Stloc, resultVar!));
                }

                if (hasEnsures)
                {
                    foreach (Instruction inst in contractBlockInstructions)
                    {
                        if (IsEndContractBlockCall(inst))
                            continue;

                        Instruction cloned = CloneInstruction(inst);

                        if (!isVoid && IsResultCall(cloned))
                        {
                            // Replace call Contract.Result<T>() with ldloc resultVar.
                            cloned = Instruction.Create(OpCodes.Ldloc, resultVar!);
                        }

                        il.InsertBefore(ret, cloned);
                    }
                }

                if (weaveInvariantOnExit)
                {
                    InsertInvariantCallBefore(il, ret, invariantMethod!);
                }

                if (!isVoid)
                {
                    il.InsertBefore(ret, Instruction.Create(OpCodes.Ldloc, resultVar!));
                }
            }
        }

        method.Body.OptimizeMacros();
        return true;
    }

    private static bool IsPublicInstanceMethodOrAccessor(MethodDefinition method)
        => method.IsPublic && !method.IsStatic;

    private static bool IsPure(TypeDefinition declaringType, MethodDefinition method)
    {
        if (HasAttribute(method, PureAttributeFullName))
            return true;

        // For accessors, also honour [Pure] on the property itself.
        if (method.IsGetter || method.IsSetter)
        {
            PropertyDefinition? prop = declaringType.Properties.FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);
            if (prop is not null && HasAttribute(prop, PureAttributeFullName))
                return true;
        }

        return false;
    }

    private static bool HasAttribute(ICustomAttributeProvider provider, string attributeFullName)
        => provider.HasCustomAttributes && provider.CustomAttributes.Any(a => a.AttributeType.FullName == attributeFullName);

    private static MethodDefinition? FindInvariantMethodOrThrow(TypeDefinition type)
    {
        List<MethodDefinition> candidates = type.Methods
            .Where(m => HasAttribute(m, OdinInvariantAttributeFullName) || HasAttribute(m, BclInvariantAttributeFullName))
            .ToList();

        if (candidates.Count == 0)
            return null;

        if (candidates.Count > 1)
        {
            string names = string.Join(", ", candidates.Select(m => m.FullName));
            throw new InvalidOperationException(
                $"Type '{type.FullName}' has multiple invariant methods. Exactly one method may be marked with ClassInvariantMethodAttribute. Candidates: {names}");
        }

        MethodDefinition invariant = candidates[0];

        if (invariant.IsStatic)
            throw new InvalidOperationException($"Invariant method must be an instance method: {invariant.FullName}");

        if (invariant.Parameters.Count != 0)
            throw new InvalidOperationException($"Invariant method must be parameterless: {invariant.FullName}");

        if (invariant.ReturnType.MetadataType != MetadataType.Void)
            throw new InvalidOperationException($"Invariant method must return void: {invariant.FullName}");

        if (!invariant.HasBody)
            throw new InvalidOperationException($"Invariant method must have a body: {invariant.FullName}");

        return invariant;
    }

    private static void InsertInvariantCallBefore(ILProcessor il, Instruction before, MethodDefinition invariantMethod)
    {
        // instance.Invariant();
        il.InsertBefore(before, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(before, Instruction.Create(OpCodes.Call, invariantMethod));
    }

    private static bool TryExtractContractBlock(MethodDefinition method, out List<Instruction> contractBlock)
    {
        // v1: contract block must be explicitly terminated by Contract.EndContractBlock().
        // This makes extraction deterministic without needing sequence points.
        contractBlock = new List<Instruction>();

        IList<Instruction> instructions = method.Body.Instructions;
        if (instructions.Count == 0)
            return false;

        int endIndex = -1;
        for (int i = 0; i < instructions.Count; i++)
        {
            Instruction inst = instructions[i];
            contractBlock.Add(inst);

            if (IsEndContractBlockCall(inst))
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex < 0)
        {
            // No explicit contract block end.
            contractBlock.Clear();
            return false;
        }

        // Also include trailing nops immediately after EndContractBlock, as they often belong
        // to the same source statement / sequence point.
        for (int i = endIndex + 1; i < instructions.Count; i++)
        {
            if (instructions[i].OpCode != OpCodes.Nop)
                break;
            contractBlock.Add(instructions[i]);
        }

        return true;
    }

    private static bool IsEnsuresCall(Instruction inst)
        => IsStaticCallToContractMethod(inst, "Ensures");

    private static bool IsEndContractBlockCall(Instruction inst)
        => IsStaticCallToContractMethod(inst, "EndContractBlock");

    private static bool IsResultCall(Instruction inst)
        => IsStaticCallToContractMethod(inst, "Result");

    private static bool IsStaticCallToContractMethod(Instruction inst, string methodName)
    {
        if (inst.OpCode != OpCodes.Call)
            return false;

        if (inst.Operand is not MethodReference mr)
            return false;

        if (mr.Name != methodName)
            return false;

        // Handle generic instance method as well.
        string declaringType = mr.DeclaringType.FullName;
        return declaringType == ContractTypeFullName;
    }

    private static Instruction CloneInstruction(Instruction inst)
    {
        // Minimal cloning sufficient for contract block statements.
        // We intentionally do not support cloning branch targets / exception handler operands in v1.
        if (inst.Operand is null)
            return Instruction.Create(inst.OpCode);

        return inst.Operand switch
        {
            sbyte b => Instruction.Create(inst.OpCode, b),
            byte b => Instruction.Create(inst.OpCode, (sbyte)b), // Cecil uses sbyte for short forms.
            int i => Instruction.Create(inst.OpCode, i),
            long l => Instruction.Create(inst.OpCode, l),
            float f => Instruction.Create(inst.OpCode, f),
            double d => Instruction.Create(inst.OpCode, d),
            string s => Instruction.Create(inst.OpCode, s),
            MethodReference mr => Instruction.Create(inst.OpCode, mr),
            FieldReference fr => Instruction.Create(inst.OpCode, fr),
            TypeReference tr => Instruction.Create(inst.OpCode, tr),
            ParameterDefinition pd => Instruction.Create(inst.OpCode, pd),
            VariableDefinition vd => Instruction.Create(inst.OpCode, vd),
            _ => throw new NotSupportedException(
                $"Unsupported operand type in contract block cloning: {inst.Operand.GetType().FullName} (opcode {inst.OpCode}).")
        };
    }
}
