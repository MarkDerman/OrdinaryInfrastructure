using NetEscapades.EnumGenerators;

namespace Odin.Temporal;

/// <summary>
/// Off the bat for V1 let's hold open the door for additional recurrence options
/// to be introduced in the future. 
/// </summary>
[EnumExtensions]
public enum RecurrenceType : short
{
    /// <summary>
    /// Daily, Weekly & Monthly recurrence.
    /// </summary>
    Default = 0,
 
    /// <summary>
    /// For example, first Friday of the month
    /// </summary>
    Relative = 1,

    /// <summary>
    /// 4-5-4, 4-4-5, etc.
    /// </summary>
    Retail = 2,  
    
    /// <summary>
    /// Hijri
    /// </summary>
    Lunar = 3 
}


/// <summary>
/// Encapsulates the details of a recurrence definition.
/// </summary>
public record RecurrenceDefinition
{
    // High-level discriminator for the parser
    public required RecurrenceType Type { get; init; }

    // Which calendar system governs the math?
    // Only Gregorian is supported, but others are "ISO8601", "Retail454", "Hijri"
    public string CalendarSystem { get; set; } = "Gregorian"; 
    
    /// <summary>
    /// How many 'units' per period? (e.g., Every '2' months)
    /// </summary>
    public int IntervalUnits { get; set; } = 1;
    
    /// <summary>
    /// Options for Default reccurence
    /// </summary>
    public DefaultRecurrenceOptions? DefaultOptions { get; init; }
}