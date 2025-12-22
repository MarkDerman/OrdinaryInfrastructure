using Mono.Cecil;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Handles type-specific matters with respect to design contract rewriting.
/// </summary>
internal class TypeHandler
{
    private readonly TypeDefinition _target;
    private MethodDefinition? _invariant;
    private bool _invariantSearched = false;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="target"></param>
    public TypeHandler(TypeDefinition target)
    {
        _target = target;
    }

    /// <summary>
    /// The enclosed Type being handled.
    /// </summary>
    public TypeDefinition Type => _target;
    
    public bool HasInvariant => InvariantMethod!=null;

    /// <summary>
    /// Null if none was found.
    /// </summary>
    public MethodDefinition? InvariantMethod
    {
        get
        {
            if (!_invariantSearched)
            {
                FindInvariantMethodOrThrow();
            }
            return _invariant;
        }
    }

    internal IReadOnlyList<MemberHandler> GetMembersToTryRewrite()
    {
        return _target.Methods.Select(c => new MemberHandler(c,this)).ToList();
    }

    internal void FindInvariantMethodOrThrow()
    {
        List<MethodDefinition> candidates = _target.Methods
            .Where(m => m.HasAnyAttributeIn(Names.InvariantAttributeFullNames))
            .ToList();
        
        _invariantSearched = true;
        
        if (candidates.Count == 0)
            return;
        
        if (candidates.Count > 1)
        {
            throw new InvalidOperationException(
                $"Type '{_target.FullName}' has multiple invariant methods. " +
                $"Exactly 1 method may be marked with either of: {string.Join(" | ", Names.InvariantAttributeFullNames)}.");
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
        
        _invariant = invariant;
    }
}