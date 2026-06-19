using TimeLedger.Core.DTOs.Event;

namespace BusinessCollaboration.DTOs.Event;

public class GroupEventResponseDto
{
    public int GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public EventResponseDto Event { get; init; } = new();
}

