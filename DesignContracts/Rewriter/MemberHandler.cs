using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Odin.System;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Handles member-specific matters with respect to design contract rewriting.
/// Note: MemberHandler includes methods AND property accessors...
/// </summary>
internal class MemberHandler
{
    private readonly TypeHandler _parentHandler;

    public MemberHandler(MethodDefinition method, TypeHandler parentHandler)
    {
        Method = method;
        _parentHandler = parentHandler;
    }
    
    public MethodDefinition Method { get; }
    
    public bool TryRewrite()
    {
        // Only handle sync (v1). We rely on analyzers to enforce this, but be defensive.
        // if (method.IsAsync)
        //     return false;
        
        // What about properties? Surely these can have no body?
        if (!Method.HasBody) return false;

        Method.Body.SimplifyMacros();
        
        InvariantWeavingRequirement invariantsToDo = IsInvariantToBeWeaved();

        ResultValue<List<Instruction>> postconditionsExtracted = 
            TryExtractPostconditions();
        
        if (!postconditionsExtracted.IsSuccess && 
            !invariantsToDo.OnEntry && !invariantsToDo.OnExit)
        {
            // Nothing to do.
            Method.Body.OptimizeMacros();
            return false;
        }

        ILProcessor il = Method.Body.GetILProcessor();

        // Inject invariant call at entry (before any user code).
        if (invariantsToDo.OnEntry)
        {
            Instruction first = Method.Body.Instructions.FirstOrDefault() ?? Instruction.Create(OpCodes.Nop);
            if (Method.Body.Instructions.Count == 0)
                il.Append(first);

            InsertInvariantCallBefore(il, first, _parentHandler.InvariantMethod!);
        }

        // Remove the postconditions from the method entry when postconditions are present.
        if (postconditionsExtracted.IsSuccess)
        {
            foreach (Instruction instruction in postconditionsExtracted.Value)
            {
                // If the instruction was already removed as part of a previous remove, skip.
                if (Method.Body.Instructions.Contains(instruction))
                    il.Remove(instruction);
            }
        }

        // If we need to inject postconditions and/or invariant calls at exit, do it per-return.
        // Todo: If there are multiple returns, create a shadow method to execute the
        // invariant and\or postconditions, calling it from each return.
        if (postconditionsExtracted.IsSuccess || invariantsToDo.OnExit)
        {
            VariableDefinition? resultVar = null;
            bool isVoid = IsVoidReturnType();
            if (!isVoid)
            {
                resultVar = new VariableDefinition(Method.ReturnType);
                Method.Body.Variables.Add(resultVar);
                Method.Body.InitLocals = true;
            }

            List<Instruction> returns = Method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
            foreach (Instruction returnInst in returns)
            {
                // For non-void methods we must preserve the return value while we call extra code.
                if (!isVoid)
                {
                    il.InsertBefore(returnInst, Instruction.Create(OpCodes.Stloc, resultVar!));
                }

                if (postconditionsExtracted.IsSuccess)
                {
                    foreach (Instruction inst in postconditionsExtracted.Value)
                    {
                        if (IsEndContractBlockCall(inst))
                            continue;

                        Instruction cloned = inst.CloneInstruction();

                        if (!isVoid && IsResultCall(cloned))
                        {
                            // Replace call Contract.Result<T>() with ldloc resultVar.
                            cloned = Instruction.Create(OpCodes.Ldloc, resultVar!);
                        }
                        il.InsertBefore(returnInst, cloned);
                    }
                }

                if (invariantsToDo.OnExit)
                {
                    InsertInvariantCallBefore(il, returnInst, _parentHandler.InvariantMethod!);
                }

                if (!isVoid)
                {
                    il.InsertBefore(returnInst, Instruction.Create(OpCodes.Ldloc, resultVar!));
                }
            }
        }

        Method.Body.OptimizeMacros();
        return true;
    }

    public InvariantWeavingRequirement IsInvariantToBeWeaved()
    {
        bool canWeaveInvariant = _parentHandler.HasInvariant && IsPublicInstanceMethod();
        bool isInvariantMethodItself = _parentHandler.InvariantMethod is not null && Method == _parentHandler.InvariantMethod;

        // Per requirements:
        // - Constructors: invariants at exit only
        // - Other public instance methods + public instance property accessors: invariants at entry and exit

        bool weaveInvariantOnEntry = canWeaveInvariant && !isInvariantMethodItself && !IsInstanceConstructor();
        bool weaveInvariantOnExit = canWeaveInvariant && !isInvariantMethodItself;

        // Exclude [Pure] methods and [Pure] properties (for accessors).
        // Ignore [Pure] on constructors...
        if (!IsInstanceConstructor() && weaveInvariantOnEntry && IsPure())
        {
            weaveInvariantOnEntry = false;
            weaveInvariantOnExit = false;
        }

        return new InvariantWeavingRequirement()
        {
            OnEntry = weaveInvariantOnEntry,
            OnExit = weaveInvariantOnExit
        };
    }
   
    public ResultValue<List<Instruction>> TryExtractPostconditions()
    {
        // For V1 we will simply attempt to extract any Postcondition.Ensures()
        // calls from the method body if they exist. MD.
        // 
        List<Instruction> postconditions = new List<Instruction>();

        IList<Instruction> instructions = Method.Body.Instructions;
        if (instructions.Count == 0)
            return ResultValue<List<Instruction>>.Failure("Method has no instructions.");

        int endIndex = -1;
        for (int i = 0; i < instructions.Count; i++)
        {
            Instruction inst = instructions[i];
            postconditions.Add(inst);

            if (IsEndContractBlockCall(inst))
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex < 0)
        {
            // No explicit contract block end.
            postconditions.Clear();
            return false;
        }

        // Also include trailing nops immediately after EndContractBlock, as they often belong
        // to the same source statement / sequence point.
        for (int i = endIndex + 1; i < instructions.Count; i++)
        {
            if (instructions[i].OpCode != OpCodes.Nop)
                break;
            postconditions.Add(instructions[i]);
        }

        return true;
    }

    
    public bool IsPostconditionEnsuresCall(Instruction inst)
        => IsStaticCallToPostconditionMethod(inst, "Ensures");

    public bool IsResultCall(Instruction inst)
        => IsStaticCallToPostconditionMethod(inst, "Result");
    
    
    public static bool IsStaticCallToPostconditionMethod(Instruction inst, string methodName)
    {
        if (inst.OpCode != OpCodes.Call)
            return false;

        if (inst.Operand is not MethodReference mr)
            return false;

        if (mr.Name != methodName)
            return false;

        // Handle generic instance method as well.
        string declaringType = mr.DeclaringType.FullName;
        return declaringType == Names.OdinPreconditionEnsuresTypeFullName;
    }
    
    /// <summary>
    /// True if Method is a public instance method.
    /// </summary>
    /// <returns></returns>
    public bool IsPublicInstanceMethod()
    {
        return Method is { IsPublic: true, IsStatic: false };
    }
    
    /// <summary>
    /// True if Method is marked as [Pure] or if it is a property accessor of a [Pure] property.
    /// </summary>
    /// <returns></returns>
    public bool IsPure()
    {
        if (Method.HasAnyAttributeIn(Names.PureAttributeFullNames))
            return true;

        // For accessors, also honour [Pure] on the property itself.
        if (Method.IsGetter || Method.IsSetter)
        {
            PropertyDefinition? prop = _parentHandler.Type.Properties.
                FirstOrDefault(p => p.GetMethod == Method || p.SetMethod == Method);
            if (prop is not null && prop.HasAnyAttributeIn(Names.PureAttributeFullNames))
                return true;
        }
        return false;
    }

    /// <summary>
    /// True if Method returns void.
    /// </summary>
    /// <returns></returns>
    public bool IsVoidReturnType()
    {
        return Method.ReturnType.MetadataType == MetadataType.Void;   
    }

    public bool IsInstanceConstructor()
    {
        return Method.IsConstructor && !Method.IsStatic;
    }
    
    private void InsertInvariantCallBefore(ILProcessor il, Instruction before, MethodDefinition invariantMethod)
    {
        // instance.Invariant();
        il.InsertBefore(before, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(before, Instruction.Create(OpCodes.Call, invariantMethod));
    }

}