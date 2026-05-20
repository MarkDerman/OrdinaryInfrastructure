using Microsoft.Extensions.DependencyInjection;

namespace Odin.Testing;

/// <summary>
/// Methods for assertion of service registration in ServiceCollection.
/// </summary>
public static class ServiceCollectionAssertions
{
    /// <summary>
    /// Verifies service registration for a serviceType, lifetime and implementation type.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="assertionAdaptor">The assertion adaptor to use for reporting failures.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="specificLifetime">The expected service lifetime.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertServiceRegistration(ServiceCollection services, IAssertionAdaptor assertionAdaptor,
        Type serviceType, ServiceLifetime specificLifetime, Type implementationType, int registrationCount = 1
        )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assertionAdaptor);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        IReadOnlyList<ServiceDescriptor> found = services.Where(x =>
            x.ServiceType == serviceType &&
            x.ImplementationType == implementationType &&
            x.Lifetime == specificLifetime).ToList();

        assertionAdaptor.AssertTrue(found.Count == registrationCount,
            $"Expected {registrationCount} registration(s) for {serviceType.Name} with lifetime {specificLifetime} " +
            $"and implementation {implementationType.Name} but found {found.Count}. "
            + GetDescriptionOfAllServicesOfType(services, serviceType));
    }

    /// <summary>
    /// Verifies service registration for a serviceType and lifetime.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="assertionAdaptor">The assertion adaptor to use for reporting failures.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="specificLifetime">The expected service lifetime.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertServiceRegistration(ServiceCollection services, IAssertionAdaptor assertionAdaptor,
        Type serviceType, ServiceLifetime specificLifetime, int registrationCount = 1
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assertionAdaptor);
        ArgumentNullException.ThrowIfNull(serviceType);

        IReadOnlyList<ServiceDescriptor> found = services.Where(x =>
            x.ServiceType == serviceType &&
            x.Lifetime == specificLifetime).ToList();

        assertionAdaptor.AssertTrue(found.Count == registrationCount,
            $"Expected {registrationCount} registration(s) for {serviceType.Name} with lifetime {specificLifetime} " +
            $"but found {found.Count}. "
            + GetDescriptionOfAllServicesOfType(services, serviceType));
    }

    /// <summary>
    /// Verifies service registration for a serviceType and implementation type with any lifetime.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    /// <param name="assertionAdaptor">The assertion adaptor to use for reporting failures.</param>
    /// <param name="serviceType">The expected service type.</param>
    /// <param name="implementationType">The expected implementation type.</param>
    /// <param name="registrationCount">The expected number of matching registrations.</param>
    public static void AssertServiceRegistration(ServiceCollection services, IAssertionAdaptor assertionAdaptor,
        Type serviceType, Type implementationType, int registrationCount = 1
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assertionAdaptor);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        IReadOnlyList<ServiceDescriptor> found = services.Where(x =>
            x.ServiceType == serviceType &&
            x.ImplementationType == implementationType).ToList();

        assertionAdaptor.AssertTrue(found.Count == registrationCount,
            $"Expected {registrationCount} registration(s) for {serviceType.Name} with any lifetime " +
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
}
