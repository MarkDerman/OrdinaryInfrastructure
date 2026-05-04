namespace Odin.Testing.XUnitV2;

/// <summary>
/// 
/// </summary>
public static class Adaptor
{
    /// <summary>
    /// 
    /// </summary>
    public static IAssertionAdaptor Instance => new XUnitV2AssertionAdaptor();
}