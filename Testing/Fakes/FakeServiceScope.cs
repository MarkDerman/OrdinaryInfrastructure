using Microsoft.Extensions.DependencyInjection;

namespace Odin.Testing;

/// <summary>
/// Uses FakeServiceProvider as the ServiceProvider.
/// </summary>
public class FakeServiceScope : IServiceScope
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeServiceScope"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve dependencies from the scope.</param>
    public FakeServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeServiceScope"/> class.
    /// </summary>
    public FakeServiceScope()
    {
        ServiceProvider = new FakeServiceProvider();
    }
    
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="IServiceProvider"/> used to resolve dependencies from the scope.
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; } 
}