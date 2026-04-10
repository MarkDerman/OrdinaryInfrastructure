using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Odin.Logging;
using Odin.Patterns.Commands;

namespace Tests.Odin.Patterns.Commands;

[TestFixture]
public sealed class DependencyInjectionExtensionsTests
{
    [Test]
    public void AddOdinCommands_registers_CommandDispatcherr_and_LoggerWrapper()
    {
        ServiceCollection services = new();

        services.AddOdinCommands();
    }


    
    [Test]
    public void AddOdinCommands_registers_dispatcher_logging_and_scanned_handlers()
    {
        ServiceCollection services = new();

        services.AddOdinCommands(typeof(DependencyInjectionExtensionsTests).Assembly);

        Assert.That(
            services.Any(x =>
                x.ServiceType == typeof(ICommandDispatcher) &&
                x.ImplementationType == typeof(ServiceProviderCommandDispatcher) &&
                x.Lifetime == ServiceLifetime.Transient),
            Is.True);

        Assert.That(
            services.Any(x =>
                x.ServiceType == typeof(ILogger<>) &&
                x.ImplementationType == typeof(NullLogger<>) &&
                x.Lifetime == ServiceLifetime.Singleton),
            Is.True);

        Assert.That(
            services.Any(x =>
                x.ServiceType == typeof(ILoggerWrapper<>) &&
                x.ImplementationType == typeof(LoggerWrapper<>) &&
                x.Lifetime == ServiceLifetime.Singleton),
            Is.True);

        Assert.That(
            services.Any(x =>
                x.ServiceType == typeof(ICommandHandler<RegisteredCommand>) &&
                x.ImplementationType == typeof(RegisteredCommandHandler) &&
                x.Lifetime == ServiceLifetime.Transient),
            Is.True);

        Assert.That(
            services.Any(x =>
                x.ServiceType == typeof(ICommandHandler<RegisteredResultCommand, string>) &&
                x.ImplementationType == typeof(RegisteredResultCommandHandler) &&
                x.Lifetime == ServiceLifetime.Transient),
            Is.True);
    }

    private sealed record RegisteredCommand(string Value) : ICommand;

    private sealed record RegisteredResultCommand(int Value) : ICommand<string>;

    private sealed class RegisteredCommandHandler : ICommandHandler<RegisteredCommand>
    {
        public Task HandleAsync(RegisteredCommand command, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class RegisteredResultCommandHandler : ICommandHandler<RegisteredResultCommand, string>
    {
        public Task<string> HandleAsync(RegisteredResultCommand command, CancellationToken ct = default)
        {
            return Task.FromResult(command.Value.ToString());
        }
    }
}
