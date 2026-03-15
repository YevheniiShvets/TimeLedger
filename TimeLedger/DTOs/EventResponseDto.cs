namespace TimeLedger.DTOs;

public class EventResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool AllowOverlap { get; set; }
    
    public string Duration
    {
        get
        {
            var duration = EndTime - StartTime;
            if (duration.TotalHours >= 1)
                return duration.TotalMinutes == 0 ?
                    $"{(int)duration.TotalHours}h" :
                    $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{duration.Minutes}m";
        }
    }
}