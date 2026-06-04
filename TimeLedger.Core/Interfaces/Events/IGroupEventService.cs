using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.DTOs.Users;

namespace TimeLedger.Core.Interfaces.Events;

public interface IGroupEventService
{
    IEnumerable<GroupEventResponseDto> GetAllForUser(int actorUserId);
    (EventResponseDto dto, bool hasOverlap, IReadOnlyList<AccountInfoDto> overlappingUsers) Create(int groupId, CreateEventDto dto, int actorUserId);
}

