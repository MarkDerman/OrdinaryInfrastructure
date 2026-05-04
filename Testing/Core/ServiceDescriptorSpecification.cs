using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Odin.Testing;

/// <summary>
/// A specification for a required service in a DI services container.
/// </summary>
public class ServiceDescriptorSpecification
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="registrationCount"></param>
    public ServiceDescriptorSpecification(Func<ServiceDescriptor, bool> criteria, int registrationCount = 1)
    {
        Criteria = criteria;
        RegistrationCount = registrationCount;
    }
    /// <summary>
    /// 
    /// </summary>
    public Func<ServiceDescriptor, bool> Criteria { get; }

    /// <summary>
    /// 
    /// </summary>
    public int RegistrationCount { get;  }

    /// <summary>
    /// Overridden. Describes the service specification.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{RegistrationCount} registration for {Criteria}";
    }
}