using TimeLedger.Models;

namespace TimeLedger.Repositories;

public interface IEventRepository
{
    IEnumerable<Event> GetAll();
    Event? GetById(int id);
    Event Add(Event e);
    Event Update(Event e);
    void Delete(Event e);
    bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId);
}