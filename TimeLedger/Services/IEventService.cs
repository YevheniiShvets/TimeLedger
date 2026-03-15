using TimeLedger.DTOs;

namespace TimeLedger.Services;

public interface IEventService
{
    Task<IEnumerable<EventResponseDto>> GetAllAsync();
    Task<EventResponseDto?> GetByIdAsync(int id);
    Task<(EventResponseDto dto, bool hasOverlap)> CreateAsync(CreateEventDto dto);
    Task<(EventResponseDto dto, bool hasOverlap)> UpdateAsync(int id, UpdateEventDto dto);
    Task DeleteAsync(int id);
}