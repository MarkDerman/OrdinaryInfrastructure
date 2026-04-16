using Microsoft.Extensions.DependencyInjection;

namespace Odin.Testing;

/// <summary>
/// Returns the IServiceScopes from the ScopeSequence property in order with
/// each call to CreateScope().
/// </summary>
public class FakeSequencedServiceScopeFactory : IServiceScopeFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeSequencedServiceScopeFactory"/> class.
    /// </summary>
    /// <param name="scopeSequence">The sequence of <see cref="IServiceScope"/> to be returned by CreateScope().</param>
    public FakeSequencedServiceScopeFactory(IEnumerable<IServiceScope>? scopeSequence = null)
    {
        ScopeSequence = scopeSequence == null
            ? new List<IServiceScope>()
            : scopeSequence.ToList();
    }
    
    /// <summary>
    /// The sequence of <see cref="IServiceScope"/> to be returned by CreateScope().
    /// </summary>
    public List<IServiceScope> ScopeSequence { get;  } 
    
    /// <summary>
    /// Index of the scope to be returned on the next call to CreateScope()
    /// </summary>
    public int ScopeSequenceIndex { get; set; } = 0;
    
    /// <summary>
    /// Returns the next <see cref="IServiceScope"/> in the <see cref="ScopeSequence"/>.
    /// </summary>
    /// <returns>The next <see cref="IServiceScope"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no more scopes to return.</exception>
    public IServiceScope CreateScope()
    {
        if (ScopeSequenceIndex >= ScopeSequence.Count)
        {
            throw new InvalidOperationException("No more scopes to return");
        }
        
        return ScopeSequence[ScopeSequenceIndex++];
    }
}