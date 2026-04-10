using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs;

public class CreateEventDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(300)]
    public string? Location { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    public bool AllowOverlap { get; set; } = false;
}