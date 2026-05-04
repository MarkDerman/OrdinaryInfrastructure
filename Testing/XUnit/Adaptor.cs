namespace Odin.Testing.XUnit;

/// <summary>
/// 
/// </summary>
public static class Adaptor
{
    /// <summary>
    /// 
    /// </summary>
    public static IAssertionAdaptor Instance => new XUnitAssertionAdaptor();
}