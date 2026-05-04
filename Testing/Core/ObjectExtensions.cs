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
    public static IReadOnlyList<object[]> AsSingleObjectsList(IEnumerable<T> testObjects)
        where T : class

    {
        return testObjects.Select(obj => new object[] { obj }).ToList();
    }
}