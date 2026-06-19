using System.ComponentModel.DataAnnotations;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Core.DTOs.Event;

public class CreateEventDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(300)]
    public string? Location { get; set; }
    
    public EventType EventType { get; set; }
    
    [Required]
    public DateTime? StartTime { get; set; }
    
    [Required]
    public DateTime? EndTime { get; set; }
    
    public DateTime? DueAt { get; set; }
    
    public bool AllowOverlap { get; set; }
    
    public RecurrenceRuleDto? RecurrenceRule { get; set; }
}