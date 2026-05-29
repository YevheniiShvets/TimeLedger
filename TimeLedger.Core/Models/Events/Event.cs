namespace TimeLedger.Core.Models.Events;

public class Event
{
    public int Id { get; set; }
    public EventOwnerType OwnerType { get; set; } = EventOwnerType.User;
    public int OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    
    public bool AllowOverlap { get; set; }
    
    public EventType EventType { get; set; } = EventType.OneTime;
    
    
    // One-time and Recurrence
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    // Deadline Event
    public DateTime? DueAt { get; set; }
    
    // Recurrence
    public RecurrenceFrequency? RecurrenceFrequency { get; set; }
    public int? RecurrenceInterval { get; set; }
    public string? RecurrenceValue { get; set; }
    
    public DateTime? RecurrenceEndTime { get; set; }
    public int? RecurrenceMaxOccurrences { get; set; }
}