using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Models.Events;

namespace TimeLedger.Core.Interfaces.Events;

public interface IEventService
{
    IEnumerable<EventResponseDto> GetAll(EventOwnerType ownerType, int ownerId);
    EventResponseDto? GetById(int id, EventOwnerType ownerType, int ownerId);
    (EventResponseDto dto, bool hasOverlap) Create(CreateEventDto dto, EventOwnerType ownerType, int ownerId);
    (EventResponseDto dto, bool hasOverlap) Update(int id, UpdateEventDto dto, EventOwnerType ownerType, int ownerId);
    void Delete(int id, EventOwnerType ownerType, int ownerId);
}