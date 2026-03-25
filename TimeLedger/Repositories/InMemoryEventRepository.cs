using TimeLedger.Models;

namespace TimeLedger.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];
    private int _nextId = 1;

    public IEnumerable<Event> GetAll()
    {
        return _events.OrderBy(e => e.StartTime);
    }

    public Event? GetById(int id)
        => _events.FirstOrDefault(e => e.Id == id);


    public Event Add(Event e)
    {
        e.Id = _nextId++;
        _events.Add(e);
        return (e);
    }

    public Event Update(Event e)
    {
        var index = _events.FindIndex(x => x.Id == e.Id);
        _events[index] = e;
        return (e);
    }

    public void Delete(Event e)
    {
        _events.Remove(e);
    }

    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId)
    {
        var result = _events.Any(e =>
            (excludeId == null || e.Id != excludeId)
            && e.StartTime < endTime
            && e.EndTime > startTime);
        return (result);
    }
}