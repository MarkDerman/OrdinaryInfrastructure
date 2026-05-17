namespace Odin.Utility;

/// <summary>
/// Represents a list of changes in the value of Value, at points along  a 1 dimensional axis of type TVaryingDimensionType.
/// </summary>
/// <typeparam name="TVaryingDimensionType">Typically DateOnly, but can be any IComparable</typeparam>
/// <typeparam name="TValueType">Declared as a class to that structs have to be passed as nullable.</typeparam>
public record ValueChange<TVaryingDimensionType, TValueType>
    where TVaryingDimensionType : IComparable
{

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="from"></param>
    /// <param name="value"></param>
    public ValueChange(TVaryingDimensionType from, TValueType value)
    {
        From = from;
        Value = value;
    }

    /// <summary>
    /// The point on the varying dimension axis above which Value applies.
    /// </summary>
    public TVaryingDimensionType From { get; init; }

    /// <summary>
    /// The value in question.
    /// </summary>
    public TValueType Value { get; init; }
}