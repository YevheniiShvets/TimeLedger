using TimeLedger.Core.Models;

namespace TimeLedger.Core.Interfaces;

public interface IEventRepository
{
    IEnumerable<Event> GetAll(int userId);
    Event? GetById(int id, int userId);
    Event Add(Event e);
    Event Update(Event e);
    void Delete(Event e);
    bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId, int userId);
}