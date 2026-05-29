using TimeLedger.Core.Models.Events;

namespace TimeLedger.Core.Interfaces.Events;

public interface IEventRepository
{
    IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId);
    Event? GetById(int id, EventOwnerType ownerType, int ownerId);
    Event Add(Event e);
    Event Update(Event e);
    void Delete(Event e);
    bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId, EventOwnerType ownerType, int ownerId);
    IEnumerable<int> GetOverlappingOwnerIds(DateTime startTime, DateTime endTime, int? excludeId, EventOwnerType ownerType, IEnumerable<int> ownerIds);
    IEnumerable<Event> GetByType(EventType type, EventOwnerType ownerType, int ownerId);
}