namespace Odin.Testing;

/// <summary>
/// Adapts framework-specific assertion APIs for shared testing utilities.
/// </summary>
public interface IAssertionAdaptor
{
    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
    /// <param name="condition">The condition that is expected to be true.</param>
    /// <param name="message">The assertion failure message.</param>
    void AssertTrue(bool condition, string message);
}
