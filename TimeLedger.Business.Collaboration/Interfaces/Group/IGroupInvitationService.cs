using BusinessCollaboration.DTOs.Group;
using TimeLedger.Core.DTOs.Inbox;

namespace BusinessCollaboration.Interfaces.Group;

public interface IGroupInvitationService
{
    InboxItemDto Invite(int groupId, CreateGroupInvitationDto dto, int userId);
    IEnumerable<InboxItemDto> GetPendingInboxForUser(int userId);
    void AcceptInvitation(int invitationId, int userId);
    void DeclineInvitation(int invitationId, int userId);
    InboxItemDto GetInvitation(int invitationId, int userId);
    void DeleteInvitation(int invitationId, int userId);
}