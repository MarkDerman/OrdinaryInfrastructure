using Odin.DesignContracts;
using Odin.Temporal.Recurrence;

namespace Odin.Temporal;

/// <summary>
/// Provides recurrence utility calculations.
/// </summary>
public interface IRecurrenceProvider
{
    /// <summary>
    /// Calculates the enclosing period for a given billing recurrence and date on which to
    /// ascertain the enclosing period.
    /// </summary>
    /// <param name="recurrenceDefinition"></param>
    /// <param name="dateToEnclose"></param>
    /// <param name="datePreviousPeriodEnded">Needed in order to support changing Recurrence</param>
    /// <returns></returns>
    Period CreateEnclosingDate(RecurrenceDefinition recurrenceDefinition, DateOnly dateToEnclose,
        DateOnly? datePreviousPeriodEnded = null);
}

/// <inheritdoc />
public class RecurrenceProvider : IRecurrenceProvider
{
    /// <inheritdoc />
    public Period CreateEnclosingDate(RecurrenceDefinition recurrenceDefinition, DateOnly dateToEnclose,
        DateOnly? datePreviousPeriodEnded = null)
    {
        Precondition.RequiresNotNull(recurrenceDefinition);
        if (datePreviousPeriodEnded.HasValue)
        {
            Precondition.Requires<ArgumentException>(datePreviousPeriodEnded.Value < dateToEnclose,
                "Previous period end date must be before the date that the period must enclose.");
        }

        if (recurrenceDefinition.Type == RecurrenceType.Default && recurrenceDefinition.DefaultOptions == null)
        {
            throw new ApplicationException($"Recurrence Options have not been set.");
        }

        if (recurrenceDefinition.DefaultOptions.Basis == DefaultRecurrenceBasis.Monthly)
        {
            // Ending is always 1 day before Day 1 of following month.
            DateOnly endingDay = new DateOnly(dateToEnclose.Year, dateToEnclose.Month, 1).AddMonths(1).AddDays(-1);

            DateOnly startingDay = new DateOnly(dateToEnclose.Year, dateToEnclose.Month, 1);
            if (datePreviousPeriodEnded.HasValue && datePreviousPeriodEnded.Value >= startingDay)
            {
                // Ensure we start 1 day later than the previous period, in case customer switched from Monthly to Weekly or vice versa.
                startingDay = datePreviousPeriodEnded.Value.AddDays(1);
            }

            return new Period(startingDay, endingDay);
        }

        if (recurrenceDefinition.DefaultOptions.Basis == DefaultRecurrenceBasis.Weekly)
        {
            DateOnly startDay;

            // DayOfWeek:
            // Sunday = 0
            // Monday = 1...
            // Tues = 2
            // Sat = 6
            // int dayOfWeek = Convert.ToInt32(currentDate.DayOfWeek);
            // Starting is current or previous Monday
            if (dateToEnclose.DayOfWeek == DayOfWeek.Sunday)
            {
                startDay = dateToEnclose.AddDays(-6);
            }
            else
            {
                startDay = dateToEnclose.AddDays(1 - Convert.ToInt32(dateToEnclose.DayOfWeek));
            }

            // Correct ending is always the Sunday, 6 days after the Monday
            DateOnly endDay = startDay.AddDays(6);

            // Correct starting is always Monday, unless previous period ending on Monday or later... 
            if (datePreviousPeriodEnded.HasValue && datePreviousPeriodEnded.Value >= startDay)
            {
                startDay = datePreviousPeriodEnded.Value.AddDays(1);
            }

            return new Period(startDay, endDay);
        }

        if (recurrenceDefinition.DefaultOptions.Basis == DefaultRecurrenceBasis.Daily)
        {
            DateOnly startingDay = new DateOnly(dateToEnclose.Year, dateToEnclose.Month, dateToEnclose.Day);

            // Ending on the same day, only one day period.
            DateOnly endingDay = startingDay;

            if (datePreviousPeriodEnded.HasValue && datePreviousPeriodEnded.Value >= startingDay)
            {
                // Ensure we start 1 day later than the previous period, in case customer switched billing cycle options
                startingDay = datePreviousPeriodEnded.Value.AddDays(1);
            }

            return new Period(startingDay, endingDay);
        }
        throw new NotSupportedException($"Recurrence : {recurrenceDefinition.Type.ToStringFast()} is not supported.");
    }
}