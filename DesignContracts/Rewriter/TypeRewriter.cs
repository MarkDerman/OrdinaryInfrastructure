using Microsoft.Build.Framework;
using Mono.Cecil;

namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// Handles Design-by-Contract rewriting of a given type.
/// </summary>
internal class TypeRewriter
{
    private readonly TypeDefinition _target;
    private MethodDefinition? _invariant;
    private bool _invariantSearched = false;
    private ILoggingAdaptor _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="target"></param>
    /// <param name="logger"></param>
    internal TypeRewriter(TypeDefinition target, ILoggingAdaptor logger)
    {
        if (target == null!) throw new ArgumentNullException(nameof(target));
        _target = target;
        _logger = logger;
    }

    /// <summary>
    /// The enclosed Type being handled.
    /// </summary>
    internal TypeDefinition Type => _target;
    
    internal ILoggingAdaptor Logger => _logger;
    
    internal bool HasInvariant => InvariantMethod!=null;

    /// <summary>
    /// Null if none was found.
    /// </summary>
    internal MethodDefinition? InvariantMethod
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

    /// <summary>
    /// Rewrites the type returning the number of members rewritten.
    /// </summary>
    /// <returns></returns>
    internal int Rewrite()
    {
        int rewritten = 0;
        foreach (var member in GetMembersToTryRewrite())
        {
            if (!member.Rewrite())
                continue;
            rewritten++;
        }
        return rewritten;
    }

    internal IReadOnlyList<MethodRewriter> GetMembersToTryRewrite()
    {
        return _target.Methods.Select(c => new MethodRewriter(c,this)).ToList();
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
        
        _logger.LogMessage(LogImportance.Low, $"Found invariant method: {invariant.FullName} to weave for type {_target.FullName}.");
        _invariant = invariant;
    }
    
}