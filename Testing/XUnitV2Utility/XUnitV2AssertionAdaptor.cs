using Xunit;

namespace Odin.Testing;

/// <summary>
/// Adapts xUnit v2 assertions for shared Odin testing utilities.
/// </summary>
public sealed class XUnitV2AssertionAdaptor : IAssertionAdaptor
{
    /// <inheritdoc />
    public void AssertTrue(bool condition, string message)
    {
        Assert.True(condition, message);
    }
}
