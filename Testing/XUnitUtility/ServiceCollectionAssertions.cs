// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Methods for assertion of service registration in ServiceCollection.
/// </summary>
public static class ServiceCollectionAssertions
{
    private static readonly Odin.Testing.IAssertionAdaptor AssertionAdaptor = new Odin.Testing.XUnitV3AssertionAdaptor();

    /// <summary>
    /// Verifies service registration for a serviceType, lifetime and implementation type.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <param name="specificLifetime">The expected service lifetime.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType,
        ServiceLifetime specificLifetime, Type implementationType, int registrationCount = 1
        )
    {
        Odin.Testing.ServiceCollectionAssertions.AssertServiceRegistration(services, AssertionAdaptor,
            serviceType, specificLifetime, implementationType, registrationCount);
    }

    /// <summary>
    /// Verifies service registration for a serviceType and lifetime.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="specificLifetime">The expected service lifetime.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType,
        ServiceLifetime specificLifetime, int registrationCount = 1
    )
    {
        Odin.Testing.ServiceCollectionAssertions.AssertServiceRegistration(services, AssertionAdaptor,
            serviceType, specificLifetime, registrationCount);
    }

    /// <summary>
    /// Verifies service registration for a serviceType and implementation type with any lifetime.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType,
        Type implementationType, int registrationCount = 1
    )
    {
        Odin.Testing.ServiceCollectionAssertions.AssertServiceRegistration(services, AssertionAdaptor,
            serviceType, implementationType, registrationCount);
    }
}
