using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs.Events;

public class UpdateEventDto
{   
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public TimeLedger.Core.Models.Events.EventType EventType { get; set; }

    [Required]
    public DateTime? StartTime { get; set; }

    [Required]
    public DateTime? EndTime { get; set; }

    public DateTime? DueAt { get; set; }

    public bool AllowOverlap { get; set; } = false;

    public RecurrenceRuleDto? RecurrenceRule { get; set; }
}