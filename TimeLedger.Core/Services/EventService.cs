using TimeLedger.Core.DTOs;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Core.Services;

public class EventService(IEventRepository repo)
{
    public IEnumerable<EventResponseDto> GetAll(int userId)
        => repo.GetAll(userId).Select(Map);

    public EventResponseDto? GetById(int id, int userId)
    {
        var e = repo.GetById(id, userId);
        return e is null ? null : Map(e);
    }

    public (EventResponseDto dto, bool hasOverlap) Create(CreateEventDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        
        ValidateLength(dto.Title, dto.Description, dto.Location); 
        
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if (repo.HasOverlap(dto.StartTime, dto.EndTime, null, userId))
                return (Map(ToEntity(dto, userId)), true);
        }

        var saved = repo.Add(ToEntity(dto, userId));
        return (Map(saved), false);
    }

    public (EventResponseDto dto, bool hasOverlap) Update(int id, UpdateEventDto dto, int userId)
    {
        var entity = repo.GetById(id, userId)
            ?? throw new KeyNotFoundException();
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        
        ValidateLength(dto.Title, dto.Description, dto.Location);
        
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if (repo.HasOverlap(dto.StartTime, dto.EndTime, id, userId))
                return (Map(entity), true);
        }

        entity.Title        = dto.Title.Trim();
        entity.Description  = dto.Description;
        entity.Location     = dto.Location;
        entity.StartTime    = dto.StartTime;
        entity.EndTime      = dto.EndTime;
        entity.AllowOverlap = dto.AllowOverlap;

        return (Map(repo.Update(entity)), false);
    }

    public void Delete(int id, int userId)
    {
        var e = repo.GetById(id, userId)
            ?? throw new KeyNotFoundException();
        repo.Delete(e);
    }

    // Private helpers
    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        
        if (start >= end)
            throw new ArgumentException("Start time must be before end time.");
    }



    private static void ValidateLength(string title, string? description, string? location)
    {
        if (title.Trim().Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.");
        if (description != null && description.Trim().Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.");
        if (location != null && location.Trim().Length > 300)
            throw new ArgumentException("Location cannot exceed 300 characters.");
    }
    
    private static EventResponseDto Map(Event e) => new() // Entity -> DTO
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        AllowOverlap = e.AllowOverlap
    };

    private static Event ToEntity(CreateEventDto d, int userId) => new() //DTO -> Entity
    {
        UserId = userId,
        Title = d.Title.Trim(),
        Description = d.Description,
        Location = d.Location,
        StartTime = d.StartTime,
        EndTime = d.EndTime,
        AllowOverlap = d.AllowOverlap
    };
}