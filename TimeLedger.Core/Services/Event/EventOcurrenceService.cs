using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Events;

namespace TimeLedger.Core.Services.Event;

public class EventOccurrenceService(IEventRepository eventRepository, IRecurrenceService recurrenceService) : IEventOccurrenceService
{
    public IEnumerable<EventResponseDto> GetOccurrencesInRange(EventOwnerType ownerType, int ownerId, DateTime rangeStart, DateTime rangeEnd)
    {
        var recurrenceEvents = eventRepository.GetByType(EventType.Recurrence, ownerType, ownerId);
        var occurrences = new List<EventResponseDto>();
        foreach (var occurrence in recurrenceEvents)
        {
            if (!occurrence.StartTime.HasValue || !occurrence.EndTime.HasValue ||
                !occurrence.RecurrenceFrequency.HasValue) continue;
            var recurrenceRule = new RecurrenceRuleDto
            {
                RecurrenceFrequency = occurrence.RecurrenceFrequency.Value,
                RecurrenceInterval = occurrence.RecurrenceInterval ?? 1,
                RecurrenceValue = occurrence.RecurrenceValue,
                RecurrenceEndTime = occurrence.RecurrenceEndTime,
                RecurrenceMaxOccurrences = occurrence.RecurrenceMaxOccurrences
            };

            var generated = recurrenceService.GenerateOccurrences(
                occurrence.StartTime.Value,
                occurrence.EndTime.Value,
                recurrenceRule,
                rangeStart,
                rangeEnd);

            foreach (var (start, end) in generated)
            {
                occurrences.Add(MapEvent(occurrence, start, end));
            }
        }
        var result = occurrences.OrderBy(o => o.StartTime).ThenBy(o => o.EndTime);
        return result;
    }

    public IEnumerable<EventResponseDto> GetOccurrencesForWeek(EventOwnerType ownerType, int ownerId, DateTime weekStartDate)
    {
        var weekEndDate = weekStartDate.AddDays(7);
        return GetOccurrencesInRange(ownerType, ownerId, weekStartDate, weekEndDate);
    }

    private static EventResponseDto MapEvent(Models.Events.Event e, DateTime startTime, DateTime endTime) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        StartTime = startTime,
        EndTime = endTime,
        OwnerType = e.OwnerType,
        OwnerId = e.OwnerId,
        EventType = e.EventType,
        RecurrenceInfo = RecurrenceInfoFormatter.Generate(e)
    };
}
/*
    1. Gets events of type recurrence from db
    2. For each event, generates occurrences in the next 3 months (including past 3 months to show recent recurrences)
    3. Maps to eventResponse
    4. Parses back to index
 */