using TimeLedger.Models;

namespace TimeLedger.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];
    private int _nextId = 1;

    public Task<IEnumerable<Event>> GetAllAsync()
        => Task.FromResult<IEnumerable<Event>>(_events.OrderBy(e => e.StartTime));

    public Task<Event?> GetByIdAsync(int id)
        => Task.FromResult(_events.FirstOrDefault(e => e.Id == id));


    public Task<Event> AddAsync(Event e)
    {
        e.Id = _nextId++;
        _events.Add(e);
        return Task.FromResult(e);
    }

    public Task<Event> UpdateAsync(Event e)
    {
        var index = _events.FindIndex(x => x.Id == e.Id);
        _events[index] = e;
        return Task.FromResult(e);
    }

    public Task DeleteAsync(Event e)
    {
        _events.Remove(e);
        return Task.CompletedTask;
    }

    public Task<bool> HasOverlapAsync(DateTime startTime, DateTime endTime, int? excludeId)
    {
        var result = _events.Any(e =>
            (excludeId == null || e.Id != excludeId)
            && e.StartTime < endTime
            && e.EndTime > startTime);
        return Task.FromResult(result);
    }
}