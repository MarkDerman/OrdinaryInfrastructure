using NetEscapades.EnumGenerators;

namespace Odin.Temporal;

/// <summary>
/// Encapsulates the details of Daily, Weekly, Monthly, or Annual recurrence.
/// </summary>
public record DefaultRecurrenceOptions
{
    public DefaultRecurrenceBasis Basis { get; init; } // Day, Week, Month //, Year
    /// <summary>
    /// Day number in the month on which recurrence starts.
    /// </summary>
    public int? DayOfMonth { get; init; } // 1-31
    
    /// <summary>
    /// Day of the week on which recurrence starts.
    /// </summary>
    public DayOfWeek? DayOfWeek { get; init; }

    /// <summary>
    /// Creates a weekly recurrence starting on the specified day of the week.
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <returns></returns>
    public static DefaultRecurrenceOptions CreateWeeklyStartingOn(DayOfWeek dayOfWeek)
    {
        return new DefaultRecurrenceOptions()
        {
            Basis = DefaultRecurrenceBasis.Weekly,
            DayOfWeek = dayOfWeek
        };
    }
    
    
    public static DefaultRecurrenceOptions CreateMonthlyStartingOn(
        int startingOnDayOfTheMonth = 1)
    {
        return new DefaultRecurrenceOptions()
        {
            Basis = DefaultRecurrenceBasis.Monthly,
            DayOfMonth = startingOnDayOfTheMonth
        };
    }
    
    public static DefaultRecurrenceOptions CreateDaily()
    {
        return new DefaultRecurrenceOptions()
        {
            Basis = DefaultRecurrenceBasis.Daily,
            DayOfMonth = null,
            DayOfWeek = null
        };
    }
}

[EnumExtensions]
public enum DefaultRecurrenceBasis : short
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2
    // , Year = 3
}
