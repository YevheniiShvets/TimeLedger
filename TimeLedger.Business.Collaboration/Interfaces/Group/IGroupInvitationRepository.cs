using BusinessCollaboration.Models.Group;

namespace BusinessCollaboration.Interfaces.Group;

public interface IGroupInvitationRepository
{
    GroupInvitation Create(GroupInvitation invitation);
    GroupInvitation? GetById(int id);
    IEnumerable<GroupInvitation> GetPendingInvitationsForUser(int userId);
    IEnumerable<GroupInvitation> GetByGroupId(int groupId);
    void Delete(int invitationId);
}