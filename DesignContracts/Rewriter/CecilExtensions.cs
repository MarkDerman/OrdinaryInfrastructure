using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Odin.DesignContracts.Rewriter;

internal static class CecilExtensions
{
    internal static bool HasAnyAttributeIn(this ICustomAttributeProvider provider, string[] attributeFullNames)
        => provider.HasCustomAttributes && provider.CustomAttributes.Any(a => attributeFullNames.Contains(a.AttributeType.FullName));

    internal static bool HasAttribute(this ICustomAttributeProvider provider, string attributeFullName)
         => provider.HasCustomAttributes && provider.CustomAttributes.Any(a => a.AttributeType.FullName == attributeFullName);

    public static bool IsPure(TypeDefinition declaringType, MethodDefinition method)
    {
        if (method.HasAnyAttributeIn(Names.PureAttributeFullNames))
            return true;

        // For accessors, also honour [Pure] on the property itself.
        if (method.IsGetter || method.IsSetter)
        {
            PropertyDefinition? prop = declaringType.Properties.FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);
            if (prop is not null && prop.HasAnyAttributeIn(Names.PureAttributeFullNames))
                return true;
        }
        return false;
    }
    
    public static Instruction CloneInstruction(this Instruction instruction)
    {
        // Minimal cloning sufficient for contract block statements.
        // We intentionally do not support cloning branch targets / exception handler operands in v1.
        if (instruction.Operand is null)
            return Instruction.Create(instruction.OpCode);

        return instruction.Operand switch
        {
            sbyte b => Instruction.Create(instruction.OpCode, b),
            byte b => Instruction.Create(instruction.OpCode, (sbyte)b), // Cecil uses sbyte for short forms.
            int i => Instruction.Create(instruction.OpCode, i),
            long l => Instruction.Create(instruction.OpCode, l),
            float f => Instruction.Create(instruction.OpCode, f),
            double d => Instruction.Create(instruction.OpCode, d),
            string s => Instruction.Create(instruction.OpCode, s),
            MethodReference mr => Instruction.Create(instruction.OpCode, mr),
            FieldReference fr => Instruction.Create(instruction.OpCode, fr),
            TypeReference tr => Instruction.Create(instruction.OpCode, tr),
            ParameterDefinition pd => Instruction.Create(instruction.OpCode, pd),
            VariableDefinition vd => Instruction.Create(instruction.OpCode, vd),
            _ => throw new NotSupportedException(
                $"Unsupported operand type in contract block cloning: {instruction.Operand.GetType().FullName} (opcode {instruction.OpCode}).")
        };
    }

}