using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Infrastructure.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];
    private int _nextId = 1;

    public IEnumerable<Event> GetAll(int userId)
    {
        return _events
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.StartTime);
    }

    public Event? GetById(int id, int userId)
        => _events.FirstOrDefault(e => e.Id == id && e.UserId == userId);


    public Event Add(Event e)
    {
        e.Id = _nextId++;
        _events.Add(e);
        return (e);
    }

    public Event Update(Event e)
    {
        var index = _events.FindIndex(x => x.Id == e.Id && x.UserId == e.UserId);
        if (index < 0)
            throw new KeyNotFoundException();
        _events[index] = e;
        return (e);
    }

    public void Delete(Event e)
    {
        _events.RemoveAll(x => x.Id == e.Id && x.UserId == e.UserId);
    }

    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId, int userId)
    {
        var result = _events.Any(e =>
            e.UserId == userId &&
            (excludeId == null || e.Id != excludeId)
            && e.StartTime < endTime
            && e.EndTime > startTime);
        return (result);
    }
}