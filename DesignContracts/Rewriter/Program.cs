using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Build-time IL rewriter that injects Design-by-Contract postconditions into method exit paths.
/// </summary>
internal static class Program
{
    private const string ContractTypeFullName = "Odin.DesignContracts.Contract";

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

    private static void RewriteAssembly(string assemblyPath, string outputPath)
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
                foreach (MethodDefinition method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    if (!TryRewriteMethod(method))
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

    private static bool TryRewriteMethod(MethodDefinition method)
    {
        // Only handle sync (v1). We rely on analyzers to enforce this, but be defensive.
        if (method.IsAsync)
            return false;

        method.Body.SimplifyMacros();

        if (!TryExtractContractBlock(method, out List<Instruction> contractBlockInstructions))
        {
            method.Body.OptimizeMacros();
            return false;
        }

        // No Ensures in contract block => nothing to rewrite.
        if (!contractBlockInstructions.Any(IsEnsuresCall))
        {
            method.Body.OptimizeMacros();
            return false;
        }

        // Remove the contract block from the method entry.
        var il = method.Body.GetILProcessor();
        foreach (Instruction inst in contractBlockInstructions)
        {
            // If the instruction was already removed as part of a previous remove, skip.
            if (method.Body.Instructions.Contains(inst))
                il.Remove(inst);
        }

        // Prepare unified epilogue and ensure all returns branch to it.
        bool isVoid = method.ReturnType.MetadataType == MetadataType.Void;
        VariableDefinition? resultVar = null;

        if (!isVoid)
        {
            resultVar = new VariableDefinition(method.ReturnType);
            method.Body.Variables.Add(resultVar);
            method.Body.InitLocals = true;
        }

        Instruction epilogueStart = Instruction.Create(OpCodes.Nop);

        // Rewrite all returns to branch to epilogueStart.
        // For non-void, store the return value into resultVar before branching.
        List<Instruction> rets = method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        foreach (Instruction ret in rets)
        {
            if (!isVoid)
            {
                il.InsertBefore(ret, Instruction.Create(OpCodes.Stloc, resultVar!));
            }

            ret.OpCode = OpCodes.Br;
            ret.Operand = epilogueStart;
        }

        // Append epilogue start.
        il.Append(epilogueStart);

        // Re-insert contract block at epilogue (excluding EndContractBlock calls).
        // Also rewrite Contract.Result<T>() to load the stored return value.
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

            il.Append(cloned);
        }

        // Final return.
        if (!isVoid)
        {
            il.Append(Instruction.Create(OpCodes.Ldloc, resultVar!));
        }

        il.Append(Instruction.Create(OpCodes.Ret));

        method.Body.OptimizeMacros();
        return true;
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
