using TimeLedger.DTOs;
using TimeLedger.Models;
using TimeLedger.Repositories;

namespace TimeLedger.Services;

public class EventService
{
    private readonly IEventRepository _repo;

    public EventService(IEventRepository repo)
    {
        _repo = repo;
    }

    public IEnumerable<EventResponseDto> GetAll()
        => _repo.GetAll().Select(Map);

    public  EventResponseDto? GetById(int id)
    {
        var e = _repo.GetById(id); // Wait for entity from repo
        return e is null ? null : Map(e);
    }

    public  (EventResponseDto dto, bool hasOverlap) Create(CreateEventDto dto)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if ( _repo.HasOverlap(dto.StartTime, dto.EndTime, null))
                return (Map(ToEntity(dto)), true);
        }

        var saved = _repo.Add(ToEntity(dto)); // Save first to get ID for response
        return (Map(saved), false);
    }

    public  (EventResponseDto dto, bool hasOverlap) Update(int id, UpdateEventDto dto)
    {
        var entity =  _repo.GetById(id) // Wait for existing entity to update
            ?? throw new KeyNotFoundException();

        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if ( _repo.HasOverlap(dto.StartTime, dto.EndTime, id))
                return (Map(entity), true); // Return existing data if overlap detected
        }

        entity.Title        = dto.Title;
        entity.Description  = dto.Description;
        entity.Location     = dto.Location;
        entity.StartTime    = dto.StartTime;
        entity.EndTime      = dto.EndTime;
        entity.AllowOverlap = dto.AllowOverlap;

        return (Map(_repo.Update(entity)), false); // Wait for update to complete and return updated data
    }

    public void Delete(int id)
    {
        var e = _repo.GetById(id)
            ?? throw new KeyNotFoundException();
        _repo.Delete(e);
    }

    // Private helpers
    private void ValidateTimeRange(DateTime start, DateTime end)
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