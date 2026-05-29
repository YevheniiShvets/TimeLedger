using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Models.Events;

namespace TimeLedger.Core.Interfaces.Events;

public interface IEventOccurrenceService
{
    IEnumerable<EventResponseDto> GetOccurrencesInRange(
        EventOwnerType ownerType,
        int ownerId,
        DateTime rangeStart,
        DateTime rangeEnd);

    IEnumerable<EventResponseDto> GetOccurrencesForWeek(
        EventOwnerType ownerType,
        int ownerId,
        DateTime weekStartDate);
}