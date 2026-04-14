using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Methods for assertion of service registration in ServiceCollection.
/// </summary>
public static class ServiceCollectionAssertions
{
    /// <summary>
    /// Verifies service registration for a serviceType, lifetime and implementation type.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
    /// <param name="specificLifetime"></param>
    /// <param name="registrationCount"></param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType,
        ServiceLifetime specificLifetime, Type implementationType, int registrationCount = 1
        )
    {
        IReadOnlyList<ServiceDescriptor> found = services.Where(x =>
            x.ServiceType == serviceType &&
            x.ImplementationType == implementationType &&
            x.Lifetime == specificLifetime).ToList();

        Assert.That(found.Count, Is.EqualTo(registrationCount), 
            $"Expected {registrationCount} registration(s) for {serviceType.Name} with lifetime {specificLifetime} " +
            $"and implementation {implementationType.Name} but found {found.Count}. " 
            + GetDescriptionOfAllServicesOfType(services, serviceType));
        
    }
    
    /// <summary>
    /// Verifies service registration for a serviceType and lifetime.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <param name="specificLifetime"></param>
    /// <param name="registrationCount"></param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType,
        ServiceLifetime specificLifetime, int registrationCount = 1
    )
    {
        IReadOnlyList<ServiceDescriptor> found = services.Where(x =>
            x.ServiceType == serviceType &&
            x.Lifetime == specificLifetime).ToList();

        Assert.That(found.Count, Is.EqualTo(registrationCount), 
            $"Expected {registrationCount} registration(s) for {serviceType.Name} with lifetime {specificLifetime} " +
            $"but found {found.Count}. " 
            + GetDescriptionOfAllServicesOfType(services, serviceType));
        
    }
    
    /// <summary>
    /// Verifies service registration for a serviceType and lifetime.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <param name="implementationType"></param>
    /// <param name="registrationCount"></param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType,
        Type implementationType, int registrationCount = 1
    )
    {
        IReadOnlyList<ServiceDescriptor> found = services.Where(x =>
            x.ServiceType == serviceType).ToList();

        Assert.That(found.Count, Is.EqualTo(registrationCount), 
            $"Expected {registrationCount} registration(s) for {serviceType.Name} with any any lifetime " +
            $"and implementation {implementationType.Name} but found {found.Count}. " 
            + GetDescriptionOfAllServicesOfType(services, serviceType));
    }
    
    private static string GetDescriptionOfAllServicesOfType(ServiceCollection services, Type serviceType)
    {
        IReadOnlyList<ServiceDescriptor> allOfType = services
            .Where(x => x.ServiceType == serviceType).ToList();
        
        string serviceTypesFound = $"{(allOfType.Count == 0 ? "No other " : allOfType.Count)} registrations found for {serviceType.Name}";
        if (allOfType.Count > 0)
        {
            serviceTypesFound += string.Join(Environment.NewLine + " - ", allOfType.Select(x => x.ToString()));
        }
        return serviceTypesFound;
    }

    

    internal static string FormatServiceDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ToString();
    }
    
}