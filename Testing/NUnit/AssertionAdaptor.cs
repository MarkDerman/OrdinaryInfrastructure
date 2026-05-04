using NUnit.Framework;

namespace Odin.Testing.NUnit;

/// <inheritdoc />
public class NUnitAssertionAdaptor : IAssertionAdaptor
{
    /// <inheritdoc />
    public void True(bool condition, string failureMessage)
    {
        Assert.True(condition, failureMessage);
    }
}