namespace TimeLedger.Core.DTOs.Events;

public class GroupEventResponse
{
    public int GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public EventResponseDto Event { get; init; } = new();
}

