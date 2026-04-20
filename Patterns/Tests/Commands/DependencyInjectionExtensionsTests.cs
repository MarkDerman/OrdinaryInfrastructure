using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Odin.Logging;
using Odin.Patterns.Commands;

namespace Tests.Odin.Patterns.Commands;

public sealed class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddOdinCommandDispatcher_registers_CommandDispatcher_and_LoggerWrapper()
    {
        ServiceCollection services = new();

        services.AddOdinCommandDispatcher();

        services.AssertServiceRegistration(typeof(ICommandDispatcher), ServiceLifetime.Transient, 
            typeof(ServiceProviderCommandDispatcher));
        services.AssertServiceRegistration(typeof(ILoggerWrapper<>), ServiceLifetime.Singleton, 
            typeof(LoggerWrapper<>));
        services.AssertServiceRegistration(typeof(ILogger<>), ServiceLifetime.Singleton);
    }

    
    [Fact]
    public void AddOdinCommandHandlers_registers_scanned_handlers()
    {
        ServiceCollection services = new();

        services.AddOdinCommandHandlers(typeof(DependencyInjectionExtensionsTests).Assembly);

        services.AssertServiceRegistration(typeof(ICommandHandler<TestCommand>), ServiceLifetime.Transient, 
            typeof(TestCommandHandler));

        services.AssertServiceRegistration(typeof(ICommandHandler<TestResultCommand, string>), ServiceLifetime.Transient, 
            typeof(TestResultCommandHandler));

    }
    
    [Fact]
    public void AddOdinCommandHandlers_requires_specific_assemblies_to_scan()
    {
        ServiceCollection services = new();

        Assert.Throws<ArgumentNullException>(() => services.AddOdinCommandHandlers(null!));
    }
}
