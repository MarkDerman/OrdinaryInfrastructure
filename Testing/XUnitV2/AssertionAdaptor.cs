using Xunit;

namespace Odin.Testing.XUnitV2;

/// <inheritdoc />
public class XUnitV2AssertionAdaptor : IAssertionAdaptor
{
    /// <inheritdoc />
    public void True(bool condition, string failureMessage)
    {
        Assert.True(condition, failureMessage);
    }
}