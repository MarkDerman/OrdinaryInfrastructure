// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Methods for assertion of service registration in ServiceCollection.
/// </summary>
public static class ServiceCollectionAssertions
{
    private static readonly Odin.Testing.IAssertionAdaptor AssertionAdaptor = new Odin.Testing.NUnitAssertionAdaptor();

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

    /// <summary>
    /// Verifies keyed service registration for a service type, key, lifetime and implementation type.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="serviceKey">The expected service key, compared using object equality. A null key matches unkeyed registrations.</param>
    /// <param name="specificLifetime">The expected service lifetime.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertKeyedServiceRegistration(this ServiceCollection services, Type serviceType,
        object? serviceKey, ServiceLifetime specificLifetime, Type implementationType, int registrationCount = 1
    )
    {
        Odin.Testing.ServiceCollectionAssertions.AssertKeyedServiceRegistration(services, AssertionAdaptor,
            serviceType, serviceKey, specificLifetime, implementationType, registrationCount);
    }

    /// <summary>
    /// Verifies keyed service registration for a service type, key and lifetime.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="serviceKey">The expected service key, compared using object equality. A null key matches unkeyed registrations.</param>
    /// <param name="specificLifetime">The expected service lifetime.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertKeyedServiceRegistration(this ServiceCollection services, Type serviceType,
        object? serviceKey, ServiceLifetime specificLifetime, int registrationCount = 1
    )
    {
        Odin.Testing.ServiceCollectionAssertions.AssertKeyedServiceRegistration(services, AssertionAdaptor,
            serviceType, serviceKey, specificLifetime, registrationCount);
    }

    /// <summary>
    /// Verifies keyed service registration for a service type, key and implementation type with any lifetime.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="serviceKey">The expected service key, compared using object equality. A null key matches unkeyed registrations.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertKeyedServiceRegistration(this ServiceCollection services, Type serviceType,
        object? serviceKey, Type implementationType, int registrationCount = 1
    )
    {
        Odin.Testing.ServiceCollectionAssertions.AssertKeyedServiceRegistration(services, AssertionAdaptor,
            serviceType, serviceKey, implementationType, registrationCount);
    }
}
