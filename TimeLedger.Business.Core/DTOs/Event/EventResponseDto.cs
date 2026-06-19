using TimeLedger.Core.Models.Event;

namespace TimeLedger.Core.DTOs.Event;

public class EventResponseDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public bool AllowOverlap { get; init; }
    public EventType EventType { get; init; }
    public EventOwnerType OwnerType { get; init; }
    public int OwnerId { get; init; }
    public DateTime? DueAt { get; init; }
    public RecurrenceFrequency? RecurrenceFrequency { get; init; }
    public int? RecurrenceInterval { get; init; }
    public DateTime? RecurrenceEndTime { get; init; }
    public int? RecurrenceMaxOccurrences { get; init; }
    public string? RecurrenceInfo { get; init; }
    
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    
    public string? Duration 
    {
        get
        {
            if (StartTime is null || EndTime is null)
                return null;
            var duration = EndTime.Value - StartTime.Value;
            if (duration.TotalDays >= 1)
            {
                return duration.Hours == 0 ? $"{(int)duration.TotalDays}d" : $"{(int)duration.TotalDays}d {duration.Hours}h"; // show days and hours
            }
            if (duration.TotalHours >= 1)
                return duration.Minutes == 0 ? $"{(int)duration.TotalHours}h" : $"{(int)duration.TotalHours}h {duration.Minutes}m"; // less than 1 day, show hours and minutes
            
            return $"{duration.Minutes}m"; // less than 1 hour, show only minutes
        }
    }
}
