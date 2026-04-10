namespace TimeLedger.Core.DTOs;

public class EventResponseDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool AllowOverlap { get; init; }
    
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
