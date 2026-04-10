namespace Odin.Testing;

/// <summary>
/// Transient, Scoped, Singleton, Any or NotRegistered.
/// </summary>
public enum ServiceRegistrationLifetimeExpectation
{
    /// <summary>
    /// Transient
    /// </summary>
    Transient,
    /// <summary>
    /// Scoped
    /// </summary>
    Scoped,
    /// <summary>
    /// Singleton
    /// </summary>
    Singleton,
    /// <summary>
    /// RegisteredWithAnyLifetime
    /// </summary>
    RegisteredWithAnyLifetime,
    /// <summary>
    /// NotRegistered
    /// </summary>
    NotRegistered
}

/// <summary>
/// Specifies types and lifetime for a single services collection registration.
/// </summary>
public class SingleRegistrationExpectation
{
    /// <summary>
    /// With no separate implementation type and any lifetime...
    /// </summary>
    /// <param name="registeredServiceType"></param>
    public SingleRegistrationExpectation(Type registeredServiceType)
    {
        RegisteredServiceType = registeredServiceType;
        Lifetime = ServiceRegistrationLifetimeExpectation.RegisteredWithAnyLifetime;
    }

    /// <summary>
    /// With any lifetime
    /// </summary>
    /// <param name="registeredServiceType"></param>
    /// <param name="implementationServiceType"></param>
    public SingleRegistrationExpectation(Type registeredServiceType, Type implementationServiceType)
    {
        RegisteredServiceType = registeredServiceType;
        ImplementationServiceType = implementationServiceType;
        Lifetime = ServiceRegistrationLifetimeExpectation.RegisteredWithAnyLifetime;
    }

    /// <summary>
    /// With a specific lifetime. Optionally with a specific implementation type.
    /// </summary>
    /// <param name="registeredServiceType"></param>
    /// <param name="lifetime"></param>
    /// <param name="implementationServiceType"></param>
    public SingleRegistrationExpectation(Type registeredServiceType,
        ServiceRegistrationLifetimeExpectation lifetime,
        Type? implementationServiceType = null)
    {
        RegisteredServiceType = registeredServiceType;
        ImplementationServiceType = implementationServiceType;
        Lifetime = lifetime;
    }

    /// <summary>
    /// The type registered, typically the abstract interface type of the service, but not necessarily.
    /// </summary>
    public Type RegisteredServiceType { get; }
    /// <summary>
    /// Optional implementation type.
    /// </summary>
    public Type? ImplementationServiceType { get; }
    
    /// <summary>
    /// Lifetime expected.
    /// </summary>
    public ServiceRegistrationLifetimeExpectation Lifetime { get; }
}