namespace Odin.Testing.NUnit;

/// <summary>
/// 
/// </summary>
public static class Adaptor
{
    /// <summary>
    /// 
    /// </summary>
    public static IAssertionAdaptor Instance => new NUnitAssertionAdaptor();
}