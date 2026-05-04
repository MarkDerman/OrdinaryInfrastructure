using Odin.Testing;
// ReSharper disable InvokeAsExtensionMember

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
    /// <param name="serviceSpecification"></param>
    public static void AssertServiceRegistration(this ServiceCollection services, 
        ServiceDescriptorSpecification serviceSpecification
    )
    {
        ServiceCollectionGenericAssertions.AssertServiceRegistration(services, 
            Adaptor.Instance, serviceSpecification);
    }
    
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
        ServiceCollectionGenericAssertions.AssertServiceRegistration(services, 
            Adaptor.Instance,serviceType,
            specificLifetime, implementationType, registrationCount);
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
        ServiceCollectionGenericAssertions.AssertServiceRegistration(services, 
            Adaptor.Instance,serviceType,
            specificLifetime, registrationCount);
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
        ServiceCollectionGenericAssertions.AssertServiceRegistration(services, 
            Adaptor.Instance,serviceType,implementationType,
            registrationCount);
    }
    
}