using NUnit.Framework;

namespace Odin.Testing;

/// <summary>
/// Adapts NUnit assertions for shared Odin testing utilities.
/// </summary>
public sealed class NUnitAssertionAdaptor : IAssertionAdaptor
{
    /// <inheritdoc />
    public void AssertTrue(bool condition, string message)
    {
        Assert.That(condition, Is.True, message);
    }
}
