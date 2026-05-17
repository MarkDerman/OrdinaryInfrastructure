namespace Odin.Utility;

/// <summary>
/// Supports the returning of a Value that may change based on another varying Dimension (eg DateOnly)
/// </summary>
/// <typeparam name="TVaryingDimensionType"></typeparam>
/// <typeparam name="TValueType"></typeparam>
public interface IVaryingValueProvider<in TVaryingDimensionType, out TValueType> where TVaryingDimensionType : IComparable
{
    /// <summary>
    /// Gets the relevant Value from the ValueChange which occurs on or before 'atPointAlongDimension' (which is most often time).
    /// </summary>
    /// <param name="atPointAlongDimension"></param>
    /// <returns></returns>
    TValueType? GetValue(TVaryingDimensionType atPointAlongDimension);
}