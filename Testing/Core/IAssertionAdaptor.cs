namespace Odin.Testing;

/// <summary>
/// Abstracts assertion helper calls so that different testing libraries and versions can be targeted.
/// </summary>
public interface IAssertionAdaptor
{
    /// <summary>
    /// Assert.True
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="failureMessage"></param>
    void True(bool condition, string failureMessage);
}