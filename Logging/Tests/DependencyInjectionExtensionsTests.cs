using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Odin.Logging;

namespace Tests.Odin.Logging;

public sealed class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddOdinLoggerWrapper_registers_LoggerWrapper_as_an_open_generic_singleton()
    {
        ServiceCollection services = new();

        services.AddOdinLoggerWrapper();

        services.AssertServiceRegistration(typeof(ILoggerWrapper<>), ServiceLifetime.Singleton, typeof(LoggerWrapper<>));
        services.AssertServiceRegistration(typeof(ILogger<>), ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddOdinLoggerWrapper_does_not_duplicate_the_wrapper_registration()
    {
        ServiceCollection services = new();

        services.AddOdinLoggerWrapper();
        services.AddOdinLoggerWrapper();

        services.AssertServiceRegistration(
            typeof(ILoggerWrapper<>),
            ServiceLifetime.Singleton,
            typeof(LoggerWrapper<>),
            registrationCount: 1);
    }
}
