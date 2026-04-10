using NUnit.Framework;
using Odin.Testing;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <param name="expectedLifetime"></param>
    /// <param name="implementationType"></param>
    public static void AssertServiceRegistration(this ServiceCollection services, Type serviceType, 
        ServiceRegistrationLifetimeExpectation? expectedLifetime = ServiceRegistrationLifetimeExpectation.RegisteredWithAnyLifetime,
        Type? implementationType = null)
    {
        if (expectedLifetime.HasValue)
        {
            Assert.That(
                services.Any(x =>
                    x.ServiceType == serviceType &&
                    x.ImplementationType == implementationType &&
                    x.Lifetime == expectedLifetime),
                Is.True);
        }
        else
        {
            Assert.That(
                services.Any(x =>
                    x.ServiceType == serviceType &&
                    x.ImplementationType == implementationType),
                Is.True);
        }
    }
}