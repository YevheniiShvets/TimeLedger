using System;
using System.Collections.Generic;
using System.Linq;
using TimeLedger.DTOs;
using TimeLedger.Models;
using TimeLedger.Repositories;

namespace TimeLedger.Services;

public class EventService(IEventRepository repo)
{
    public IEnumerable<EventResponseDto> GetAll()
        => repo.GetAll().Select(Map);

    public  EventResponseDto? GetById(int id)
    {
        var e = repo.GetById(id);
        return e is null ? null : Map(e);
    }
    public  (EventResponseDto dto, bool hasOverlap) Create(CreateEventDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        
        ValidateLength(dto.Title, dto.Description, dto.Location); 
        
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if ( repo.HasOverlap(dto.StartTime, dto.EndTime, null))
                return (Map(ToEntity(dto)), true);
        }

        var saved = repo.Add(ToEntity(dto));
        return (Map(saved), false);
    }

    public  (EventResponseDto dto, bool hasOverlap) Update(int id, UpdateEventDto dto)
    {
        var entity =  repo.GetById(id)
            ?? throw new KeyNotFoundException();
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        
        ValidateLength(dto.Title, dto.Description, dto.Location);
        
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (!dto.AllowOverlap)
        {
            if ( repo.HasOverlap(dto.StartTime, dto.EndTime, id))
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

    public void Delete(int id)
    {
        var e = repo.GetById(id)
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

    private static Event ToEntity(CreateEventDto d) => new() //DTO -> Entity
    {
        Title = d.Title.Trim(),
        Description = d.Description,
        Location = d.Location,
        StartTime = d.StartTime,
        EndTime = d.EndTime,
        AllowOverlap = d.AllowOverlap
    };
}