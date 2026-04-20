namespace Tests.Odin.Email;

/// <summary>
///  Base class for integration tests or any test needing application
/// configuration, or access to DI services.
/// </summary>
public abstract class IntegrationTest
{
    protected readonly TestApplicationFactory AppFactory = new();
}
