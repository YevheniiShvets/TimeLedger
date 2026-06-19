using TimeLedger.Core.Models.Event;

namespace TimeLedger.Core.Services.Event;

public static class RecurrenceInfoFormatter //Utility class with just formatting logic, no state
{
    public static string? Generate(Models.Event.Event e)
    {
        if (e.EventType != EventType.Recurrence || e.RecurrenceFrequency == null)
            return null;

        var interval = e.RecurrenceInterval ?? 1;
        var unit = GetRecurrenceUnit(e.RecurrenceFrequency.Value, interval == 1);
        var info = interval == 1 ? unit : $"every {interval} {unit}";

        if (e.RecurrenceMaxOccurrences.HasValue)
            info += $" ({e.RecurrenceMaxOccurrences} times)";
        else if (e.RecurrenceEndTime.HasValue)
            info += $" until {e.RecurrenceEndTime:d}";

        return info;
    }

    private static string GetRecurrenceUnit(RecurrenceFrequency frequency, bool singular)
        => frequency switch
        {
            RecurrenceFrequency.Daily => singular ? "daily" : "days",
            RecurrenceFrequency.Weekly => singular ? "weekly" : "weeks",
            RecurrenceFrequency.Monthly => singular ? "monthly" : "months",
            RecurrenceFrequency.Yearly => singular ? "yearly" : "years",
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), "Invalid recurrence frequency.")
        };
}

