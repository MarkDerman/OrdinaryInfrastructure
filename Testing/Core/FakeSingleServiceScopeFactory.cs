using Microsoft.Extensions.DependencyInjection;

namespace Odin.Testing;

/// <summary>
/// Always returns the same IServiceScope in Scope when calling CreateScope.
/// </summary>
public class FakeSingleServiceScopeFactory : IServiceScopeFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeSingleServiceScopeFactory"/> class with the specified services.
    /// </summary>
    /// <param name="withServices">The collection of services to be available within the scope.</param>
    public FakeSingleServiceScopeFactory(object[] withServices)
    {
        var provider = new FakeServiceProvider(withServices);
        Scope = new FakeServiceScope(provider);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeSingleServiceScopeFactory"/> class with an empty <see cref="FakeServiceProvider"/>.
    /// </summary>
    public FakeSingleServiceScopeFactory()
    {
        Scope = new FakeServiceScope();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeSingleServiceScopeFactory"/> class with a specific <see cref="IServiceScope"/>.
    /// </summary>
    /// <param name="scope">The <see cref="IServiceScope"/> to be returned by CreateScope().</param>
    public FakeSingleServiceScopeFactory(IServiceScope scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// Gets or sets the <see cref="IServiceScope"/> that is always returned by <see cref="CreateScope"/>.
    /// </summary>
    public IServiceScope Scope { get; set; }

    /// <summary>
    /// Creates a new <see cref="IServiceScope"/> which is always the same <see cref="Scope"/>.
    /// </summary>
    /// <returns>The fixed <see cref="IServiceScope"/>.</returns>
    public IServiceScope CreateScope()
    {
        return Scope;
    }
}