using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models;
using TimeLedger.Core.Models.Events;

namespace TimeLedger.Infrastructure.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];
    private int _nextId = 1;

    public IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId)
    {
        return _events
            .Where(e => e.OwnerType == ownerType && e.OwnerId == ownerId)
            .OrderBy(e => e.StartTime);
    }

    public Event? GetById(int id, EventOwnerType ownerType, int ownerId)
        => _events.FirstOrDefault(e => e.Id == id && e.OwnerType == ownerType && e.OwnerId == ownerId);


    public Event Add(Event e)
    {
        e.Id = _nextId++;
        _events.Add(e);
        return (e);
    }

    public Event Update(Event e)
    {
        var index = _events.FindIndex(x => x.Id == e.Id && x.OwnerType == e.OwnerType && x.OwnerId == e.OwnerId);
        if (index < 0)
            throw new KeyNotFoundException();
        _events[index] = e;
        return (e);
    }

    public void Delete(Event e)
    {
        _events.RemoveAll(x => x.Id == e.Id && x.OwnerType == e.OwnerType && x.OwnerId == e.OwnerId);
    }

    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId, EventOwnerType ownerType, int ownerId)
    {
        var result = _events.Any(e =>
            e.OwnerType == ownerType &&
            e.OwnerId == ownerId &&
            (excludeId == null || e.Id != excludeId)
            && e.StartTime < endTime
            && e.EndTime > startTime);
        return (result);
    }
}