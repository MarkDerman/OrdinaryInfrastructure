using Xunit;

namespace Odin.Testing;

/// <summary>
/// Adapts xUnit v3 assertions for shared Odin testing utilities.
/// </summary>
public sealed class XUnitV3AssertionAdaptor : IAssertionAdaptor
{
    /// <inheritdoc />
    public void AssertTrue(bool condition, string message)
    {
        Assert.True(condition, message);
    }
}
