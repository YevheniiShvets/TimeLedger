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
        // 1d 0h 1m, 1d 1h 0m, 1d 0h 0m, 0d 1h 1m, 0d 1h 0m, 0d 0h 1m
        get
        {
            var duration = EndTime - StartTime;
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
