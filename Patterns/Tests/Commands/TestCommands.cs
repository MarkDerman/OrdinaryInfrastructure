using Odin.Patterns.Commands;

namespace Tests.Odin.Patterns.Commands;

// ReSharper disable once ClassNeverInstantiated.Local
internal sealed record TestCommand(string Value) : ICommand;

// ReSharper disable once ClassNeverInstantiated.Local
internal sealed record TestResultCommand(int Value) : ICommand<string>;

internal sealed class TestCommandHandler : ICommandHandler<TestCommand>
{
    public Task HandleAsync(TestCommand command, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}

internal sealed class TestResultCommandHandler : ICommandHandler<TestResultCommand, string>
{
    public Task<string> HandleAsync(TestResultCommand command, CancellationToken ct = default)
    {
        return Task.FromResult(command.Value.ToString());
    }
}
