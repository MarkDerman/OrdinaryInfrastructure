using Odin.DesignContracts;

namespace Odin.Temporal;

/// <summary>
/// Represents a period with a starting and ending date.
/// </summary>
public record Period
{
    public Period(DateOnly starting, DateOnly ending)
    {
        Precondition.Requires(starting <= ending, "Starting date must be before or equal to ending date.");
        Starting = starting;
        Ending = ending;
    }
    public DateOnly Starting { get; init; }
    public DateOnly Ending { get; init; }
}