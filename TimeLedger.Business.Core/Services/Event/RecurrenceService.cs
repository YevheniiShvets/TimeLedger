using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Core.Services.Event;

public class RecurrenceService : IRecurrenceService
{
    public IEnumerable<(DateTime start, DateTime end)> GenerateOccurrences(
        DateTime eventStart,
        DateTime eventEnd,
        RecurrenceRuleDto recurrence,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        if (eventEnd <= eventStart)
            return [];

        var interval = recurrence.RecurrenceInterval < 1 ? 1 : recurrence.RecurrenceInterval;
        var stopAt = recurrence.RecurrenceEndTime ?? DateTime.Now.AddDays(100);
        var maxOccurrences = recurrence.RecurrenceMaxOccurrences ?? 100;
        var duration = eventEnd - eventStart;

        var occurrences = new List<(DateTime start, DateTime end)>();
        var currentStart = eventStart;
        var occurrenceIndex = 0; 

        while (currentStart <= stopAt && occurrenceIndex < maxOccurrences)
        {
            var currentEnd = currentStart.Add(duration);
            
            if (currentStart > rangeEnd)
                break;

            if (currentEnd > rangeStart && currentStart < rangeEnd)
                occurrences.Add((currentStart, currentEnd));

            var nextStart = GetNextStart(currentStart, eventStart, recurrence, interval);
            if (nextStart <= currentStart)
                break;

            currentStart = nextStart;
            occurrenceIndex++;
        }

        return occurrences;
    }

    private static DateTime GetNextStart(DateTime currentStart, DateTime originalStart, RecurrenceRuleDto recurrence, int interval)
    {
        return recurrence.RecurrenceFrequency switch
        {
            RecurrenceFrequency.Daily => currentStart.AddDays(interval),
            RecurrenceFrequency.Weekly => GetWeeklyNextStart(currentStart, originalStart, recurrence.RecurrenceValue, interval),
            RecurrenceFrequency.Monthly => GetMonthlyNextStart(currentStart, originalStart, recurrence.RecurrenceValue, interval),
            RecurrenceFrequency.Yearly => currentStart.AddYears(interval),
            _ => currentStart
        };
    }

    private static DateTime GetWeeklyNextStart(DateTime currentStart, DateTime originalStart, string? recurrenceValue, int interval)
    {
        var allowedDays = ParseWeekdays(recurrenceValue);
        if (allowedDays.Length == 0)
            return currentStart.AddDays(7 * interval);

        var candidate = currentStart.AddDays(1);
        var maxSearchDays = 7 * interval;

        for (var i = 0; i < maxSearchDays; i++)
        {
            if (Array.IndexOf(allowedDays, (int)candidate.DayOfWeek) >= 0)
                return candidate;

            candidate = candidate.AddDays(1);
        }

        return originalStart.AddDays(7 * interval);
    }

    private static DateTime GetMonthlyNextStart(DateTime currentStart, DateTime originalStart, string? recurrenceValue, int interval)
    {
        var nextMonth = currentStart.AddMonths(interval);
        var dayOfMonth = ParseDayOfMonth(recurrenceValue) ?? originalStart.Day;
        var clampedDay = Math.Min(dayOfMonth, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));

        return new DateTime(
            nextMonth.Year,
            nextMonth.Month,
            clampedDay,
            originalStart.Hour,
            originalStart.Minute,
            originalStart.Second,
            originalStart.Kind);
    }

    private static int[] ParseWeekdays(string? recurrenceValue)
    {
        if (string.IsNullOrWhiteSpace(recurrenceValue))
            return [];

        return recurrenceValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => int.TryParse(part, out var day) && day is >= 0 and <= 6 ? day : -1)
            .Where(day => day >= 0)
            .Distinct()
            .ToArray();
    }

    private static int? ParseDayOfMonth(string? recurrenceValue)
    {
        if (int.TryParse(recurrenceValue, out var day) && day is >= 1 and <= 31)
            return day;

        return null;
    }
}