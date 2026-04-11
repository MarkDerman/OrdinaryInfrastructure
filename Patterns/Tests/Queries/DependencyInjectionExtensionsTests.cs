using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Odin.Logging;
using Odin.Patterns.Queries;

namespace Tests.Odin.Patterns.Queries;

[TestFixture]
public sealed class DependencyInjectionExtensionsTests
{
    [Test]
    public void AddOdinQueryDispatcher_registers_QueryDispatcher_and_LoggerWrapper()
    {
        ServiceCollection services = new();

        services.AddOdinQueryDispatcher();

        services.AssertServiceRegistration(typeof(IQueryDispatcher), ServiceLifetime.Transient,
            typeof(ServiceProviderQueryDispatcher));
        services.AssertServiceRegistration(typeof(ILoggerWrapper<>), ServiceLifetime.Singleton,
            typeof(LoggerWrapper<>));
        services.AssertServiceRegistration(typeof(ILogger<>), ServiceLifetime.Singleton);
    }

    [Test]
    public void AddOdinQueryHandlers_registers_scanned_handlers()
    {
        ServiceCollection services = new();

        services.AddOdinQueryHandlers(typeof(DependencyInjectionExtensionsTests).Assembly);

        services.AssertServiceRegistration(typeof(IQueryHandler<TestQuery, string>), ServiceLifetime.Transient,
            typeof(TestQueryHandler));
        services.AssertServiceRegistration(typeof(IQueryHandler<AlternateTestQuery, int>), ServiceLifetime.Transient,
            typeof(AlternateTestQueryHandler));
    }

    [Test]
    public void AddOdinQueryHandlers_requires_specific_assemblies_to_scan()
    {
        ServiceCollection services = new();

        Assert.Catch<ArgumentNullException>(() => services.AddOdinQueryHandlers(null!));
    }
}
