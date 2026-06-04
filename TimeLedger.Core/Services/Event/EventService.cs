using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Events;

namespace TimeLedger.Core.Services.Event;

public class EventService(IEventRepository repo) : IEventService
{
    public IEnumerable<EventResponseDto> GetAll(EventOwnerType ownerType, int ownerId)
        => repo.GetAll(ownerType, ownerId).Select(Map);

    public EventResponseDto? GetById(int id, EventOwnerType ownerType, int ownerId)
    {
        var e = repo.GetById(id, ownerType, ownerId);
        return e == null ? null : Map(e);
    }

    public (EventResponseDto dto, bool hasOverlap) Create(CreateEventDto dto, EventOwnerType ownerType, int ownerId)
    {

        ValidateFields(dto);

        if (!dto.AllowOverlap && dto.EventType != EventType.Deadline)
        {
            if (repo.HasOverlap(dto.StartTime!.Value, dto.EndTime!.Value, null, ownerType, ownerId))
                return (Map(ToEntity(dto, ownerType, ownerId)), true);
        }

        var saved = repo.Add(ToEntity(dto, ownerType, ownerId));
        return (Map(saved), false);
    }

    public (EventResponseDto dto, bool hasOverlap) Update(int id, UpdateEventDto dto, EventOwnerType ownerType, int ownerId)
    {
        var entity = repo.GetById(id, ownerType, ownerId)
            ?? throw new KeyNotFoundException();
        
        ValidateFields(dto);

        if (!dto.AllowOverlap && dto.EventType != EventType.Deadline) //Problem 1 was checking for entity instead of dto (idk why I did that)
        {
            if (repo.HasOverlap(dto.StartTime!.Value, dto.EndTime!.Value, id, ownerType, ownerId))
                return (Map(entity), true);
        }

        var saved = repo.Update(ToEntity(dto, ownerType, ownerId, id));

        return (Map(saved), false);
    }

    public void Delete(int id, EventOwnerType ownerType, int ownerId)
    {
        var e = repo.GetById(id, ownerType, ownerId)
            ?? throw new KeyNotFoundException();
        repo.Delete(e);
    }

    
    
    
    // Private helpers
    
    private static void ValidateFields(UpdateEventDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        if (dto.Title.Trim().Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.");
        if (dto.Description != null && dto.Description.Trim().Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.");
        if (dto.Location != null && dto.Location.Trim().Length > 300)
            throw new ArgumentException("Location cannot exceed 300 characters.");
        
        if (dto.EventType == EventType.Deadline)
        {
            if (!dto.DueAt.HasValue)
                throw new ArgumentException("Due date is required for deadline events.");
        }
        else
        {
            ValidateTimeRange(dto.StartTime, dto.EndTime);
        }
    }
    private static void ValidateFields(CreateEventDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        if (dto.Title.Trim().Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.");
        if (dto.Description != null && dto.Description.Trim().Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.");
        if (dto.Location != null && dto.Location.Trim().Length > 300)
            throw new ArgumentException("Location cannot exceed 300 characters.");
        
        if (dto.EventType == EventType.Deadline)
        {
            if (!dto.DueAt.HasValue)
                throw new ArgumentException("Due date is required for deadline events.");
        }
        else
        {
            ValidateTimeRange(dto.StartTime, dto.EndTime);
        }
    }
    
    private static void ValidateTimeRange(DateTime? start, DateTime? end)
    {
        if (!start.HasValue || !end.HasValue)
            throw new ArgumentException("Start time and end time are required.");

        if (start >= end)
            throw new ArgumentException("Start time must be before end time.");
    }
    
    private static EventResponseDto Map(Models.Events.Event e) => new() // Entity -> DTO
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        AllowOverlap = e.AllowOverlap,
        EventType = e.EventType,
        DueAt = e.DueAt,
        RecurrenceFrequency = e.RecurrenceFrequency,
        RecurrenceInterval = e.RecurrenceInterval,
        RecurrenceEndTime = e.RecurrenceEndTime,
        RecurrenceMaxOccurrences = e.RecurrenceMaxOccurrences,
        RecurrenceInfo = RecurrenceInfoFormatter.Generate(e)
    };

    private static Models.Events.Event ToEntity(CreateEventDto dto, EventOwnerType ownerType, int ownerId) => new() //DTO -> Entity (create)
    {
        OwnerType = ownerType,
        OwnerId = ownerId,
        Title = dto.Title.Trim(),
        Description = dto.Description,
        Location = dto.Location,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        AllowOverlap = dto.AllowOverlap,
        EventType = dto.EventType,
        DueAt = dto.DueAt,
        RecurrenceFrequency = dto.RecurrenceRule?.RecurrenceFrequency,
        RecurrenceInterval = dto.RecurrenceRule?.RecurrenceInterval,
        RecurrenceValue = dto.RecurrenceRule?.RecurrenceValue,
        RecurrenceEndTime = dto.RecurrenceRule?.RecurrenceEndTime,
        RecurrenceMaxOccurrences = dto.RecurrenceRule?.RecurrenceMaxOccurrences
    };
    private static Models.Events.Event ToEntity(UpdateEventDto dto, EventOwnerType ownerType, int ownerId, int eventId) => new() //DTO -> Entity (update)
    {
        Id = eventId,
        OwnerType = ownerType,
        OwnerId = ownerId,
        Title = dto.Title.Trim(),
        Description = dto.Description,
        Location = dto.Location,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        AllowOverlap = dto.AllowOverlap,
        EventType = dto.EventType,
        DueAt = dto.DueAt,
        RecurrenceFrequency = dto.RecurrenceRule?.RecurrenceFrequency,
        RecurrenceInterval = dto.RecurrenceRule?.RecurrenceInterval,
        RecurrenceValue = dto.RecurrenceRule?.RecurrenceValue,
        RecurrenceEndTime = dto.RecurrenceRule?.RecurrenceEndTime,
        RecurrenceMaxOccurrences = dto.RecurrenceRule?.RecurrenceMaxOccurrences
     };
}