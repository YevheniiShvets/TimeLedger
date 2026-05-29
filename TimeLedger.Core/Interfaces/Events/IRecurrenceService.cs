using TimeLedger.Core.DTOs.Events;

namespace TimeLedger.Core.Interfaces.Events;

public interface IRecurrenceService
{
    IEnumerable<(DateTime start, DateTime end)> GenerateOccurrences(
        DateTime eventStart,
        DateTime eventEnd,
        RecurrenceRuleDto recurrence,
        DateTime rangeStart,
        DateTime rangeEnd);
}