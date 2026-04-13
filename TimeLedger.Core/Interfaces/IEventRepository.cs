using TimeLedger.Core.Models;

namespace TimeLedger.Core.Interfaces;

public interface IEventRepository
{
    IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId);
    Event? GetById(int id, EventOwnerType ownerType, int ownerId);
    Event Add(Event e);
    Event Update(Event e);
    void Delete(Event e);
    bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId, EventOwnerType ownerType, int ownerId);
}