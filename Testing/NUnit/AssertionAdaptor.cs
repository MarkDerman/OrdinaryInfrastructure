using NUnit.Framework;

namespace Odin.Testing;

/// <inheritdoc />
internal class AssertionAdaptor : IAssertionAdaptor
{
    /// <summary>
    /// 
    /// </summary>
    public static IAssertionAdaptor Instance => new AssertionAdaptor();

    /// <inheritdoc />
    public void True(bool condition, string failureMessage)
    {
        Assert.True(condition, failureMessage);
    }
}