using TimeLedger.DTOs;
using TimeLedger.Models;
using TimeLedger.Repositories;

namespace TimeLedger.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repo;

    public EventService(IEventRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<EventResponseDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map);

    public async Task<EventResponseDto?> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : Map(e);
    }

    public async Task<(EventResponseDto dto, bool hasOverlap)> CreateAsync(CreateEventDto dto)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if (await _repo.HasOverlapAsync(dto.StartTime, dto.EndTime, null))
                return (Map(ToEntity(dto)), true);
        }

        var saved = await _repo.AddAsync(ToEntity(dto));
        return (Map(saved), false);
    }

    public async Task<(EventResponseDto dto, bool hasOverlap)> UpdateAsync(int id, UpdateEventDto dto)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException();

        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if (await _repo.HasOverlapAsync(dto.StartTime, dto.EndTime, id))
                return (Map(entity), true); // Return existing data if overlap detected
        }

        entity.Title        = dto.Title;
        entity.Description  = dto.Description;
        entity.Location     = dto.Location;
        entity.StartTime    = dto.StartTime;
        entity.EndTime      = dto.EndTime;
        entity.AllowOverlap = dto.AllowOverlap;

        return (Map(await _repo.UpdateAsync(entity)), false);
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException();
        await _repo.DeleteAsync(e);
    }

    // Private helpers
    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Start time must be before end time.");
    }

    private static EventResponseDto Map(Event e) => new() // Entity -> DTO
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        AllowOverlap = e.AllowOverlap,
    };

    private static Event ToEntity(CreateEventDto d) => new() //DTO -> Entity
    {
        Title = d.Title,
        Description = d.Description,
        Location = d.Location,
        StartTime = d.StartTime,
        EndTime = d.EndTime,
        AllowOverlap = d.AllowOverlap,
    };
}