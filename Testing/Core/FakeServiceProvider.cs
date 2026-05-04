namespace Odin.Testing;

/// <summary>
///     This exists because you cannot call e.g. new Mock IServiceProvider ().Setup(m => m.GetRequiredService....
///     Since GetRequiredService, GetRequiredKeyedService, etc. are static extension methods.
/// </summary>
public class FakeServiceProvider : IServiceProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeServiceProvider"/> class.
    /// </summary>
    /// <param name="services">The initial collection of services.</param>
    public FakeServiceProvider(IEnumerable<object>? services = null)
    {
        Services = services==null ? [] : services.ToList();
    }
    
    /// <summary>
    /// Gets or sets the collection of services.
    /// Any service that you want GetService to find must be added in here.
    /// </summary>
    public List<object> Services { get; set; } 

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType"/>; or <see langword="null"/> if there is no service object of type <paramref name="serviceType"/>.</returns>
    public object? GetService(Type serviceType)
    {
        foreach (var service in Services)
        {
            if (service.GetType().IsAssignableTo(serviceType))
            {
                return service;
            }
        }

        return null;
    }
}