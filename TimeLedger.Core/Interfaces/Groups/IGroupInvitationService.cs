using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.DTOs.Inbox;

namespace TimeLedger.Core.Interfaces.Groups;

public interface IGroupInvitationService
{
    InboxItemDto Invite(int groupId, CreateGroupInvitationDto dto, int userId);
    IEnumerable<InboxItemDto> GetPendingInboxForUser(int userId);
    void AcceptInvitation(int invitationId, int userId);
    void DeclineInvitation(int invitationId, int userId);
    InboxItemDto GetInvitation(int invitationId, int userId);
    void DeleteInvitation(int invitationId, int userId);
}