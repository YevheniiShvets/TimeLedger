using TimeLedger.Core.Models.Event;

namespace TimeLedger.Core.Interfaces.Events;

public interface IEventRepository
{
    IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId);
    Event? GetById(int id, EventOwnerType ownerType, int ownerId);
    Event Add(Event e);
    Event Update(Event e);
    void Delete(Event e);
    bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId, EventOwnerType ownerType, int ownerId);
    IEnumerable<Event> GetByType(EventType type, EventOwnerType ownerType, int ownerId);
    IEnumerable<Event> GetUpdatedSince(DateTime since, EventOwnerType ownerType, int ownerId);
    Event Upsert(Event e);
}