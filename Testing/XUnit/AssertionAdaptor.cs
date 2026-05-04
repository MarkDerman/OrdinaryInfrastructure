using Xunit;

namespace Odin.Testing.XUnit;

/// <inheritdoc />
public class XUnitAssertionAdaptor : IAssertionAdaptor
{


    /// <inheritdoc />
    public void True(bool condition, string failureMessage)
    {
        Assert.True(condition, failureMessage);
    }
}