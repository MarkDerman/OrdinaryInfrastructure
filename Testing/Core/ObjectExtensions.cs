namespace Odin.Testing;

/// <summary>
/// Useful for XUnit Memberdata
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Useful for XUnit Memberdata
    /// </summary>
    /// <param name="testObjects"></param>
    /// <returns></returns>
    public static IReadOnlyList<object[]> AsListOfObjectArray(IEnumerable<object> testObjects)
    {
        return testObjects.Select(obj => new object[] { obj }).ToList();
    }
}