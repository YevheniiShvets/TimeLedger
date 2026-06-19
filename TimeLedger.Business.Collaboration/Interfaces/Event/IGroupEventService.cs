using BusinessCollaboration.DTOs.Event;
using BusinessCollaboration.DTOs.User;
using TimeLedger.Core.DTOs.Event;

namespace BusinessCollaboration.Interfaces.Event;

public interface IGroupEventService
{
    IEnumerable<GroupEventResponseDto> GetAllForUser(int actorUserId);
    (EventResponseDto dto, bool hasOverlap, IReadOnlyList<AccountInfoDto> overlappingUsers) Create(int groupId, CreateEventDto dto, int actorUserId);
}

