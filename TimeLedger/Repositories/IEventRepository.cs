using TimeLedger.Models;

namespace TimeLedger.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task<Event> AddAsync(Event e);
    Task<Event> UpdateAsync(Event e);
    Task DeleteAsync(Event e);
    Task<bool> HasOverlapAsync(DateTime startTime, DateTime endTime, int? excludeId);
}