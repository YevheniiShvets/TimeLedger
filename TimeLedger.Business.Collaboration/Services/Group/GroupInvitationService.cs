using BusinessCollaboration.DTOs.Group;
using BusinessCollaboration.Interfaces.Group;
using BusinessCollaboration.Interfaces.User;
using BusinessCollaboration.Models.Group;
using TimeLedger.Core.DTOs.Inbox;

namespace BusinessCollaboration.Services.Group;

public class GroupInvitationService(IGroupInvitationRepository invitationRepo, IUserRepository userRepo, IGroupRepository groupRepo) : IGroupInvitationService
{
    
    public InboxItemDto Invite(int groupId, CreateGroupInvitationDto dto, int actorUserId)
    {
        if(dto.InviteeEmail == null)
            throw new ArgumentException("Invitee email is required.");
        
        var user = userRepo.GetByEmail(dto.InviteeEmail);
        var actorUser = userRepo.GetById(actorUserId);
        var group = groupRepo.GetGroupById(groupId, actorUserId);
        
        ValidateUser(dto, user, actorUser, group);
        
        var userInvitations = invitationRepo.GetPendingInvitationsForUser(user.Id);
        if (userInvitations.Any(inv => inv.GroupId == groupId))
            throw new InvalidOperationException($"User {user.Id} already has a pending invitation for group {groupId}.");
        
        var invitation = invitationRepo.Create(ToEntity(user.Id, actorUserId, groupId));
        return Map(invitation, actorUser.Name, group.Name);
    }

    public IEnumerable<InboxItemDto> GetPendingInboxForUser(int userId)
    {
        var invitations = invitationRepo.GetPendingInvitationsForUser(userId).ToList();
        
        var actors = invitations.Select(inv => userRepo.GetById(inv.InvitedByUserId)?.Name ?? "Unknown").ToList();
        var groups = invitations.Select(inv => groupRepo.GetGroupById(inv.GroupId, inv.InvitedByUserId).Name).ToList();
        return invitations.Select((inv, index) => Map(inv, actors[index], groups[index])).ToList();
    }

    public void AcceptInvitation(int invitationId, int userId)
    {
        var invitation = invitationRepo.GetById(invitationId);
        if (invitation == null || invitation.InvitedUserId != userId)
            throw new InvalidOperationException("Invitation not found or access denied.");
        
        groupRepo.AddGroupMember(invitation.GroupId, userId);
        invitationRepo.Delete(invitationId);
    }

    public void DeclineInvitation(int invitationId, int userId)
    {
        var invitation = invitationRepo.GetById(invitationId);
        if (invitation == null || invitation.InvitedUserId != userId)
            throw new InvalidOperationException("Invitation not found or access denied.");
        
        invitationRepo.Delete(invitationId);
    }

    public InboxItemDto GetInvitation(int invitationId, int userId)
    {
        var invitation = invitationRepo.GetById(invitationId);
        if (invitation == null || invitation.InvitedUserId != userId)
            throw new InvalidOperationException("Invitation not found or access denied.");
        
        var actorUser = userRepo.GetById(invitation.InvitedByUserId);
        var group = groupRepo.GetGroupById(invitation.GroupId, invitation.InvitedByUserId);
        
        return Map(invitation, actorUser?.Name ?? "Unknown", group.Name);
    }

    public void DeleteInvitation(int invitationId, int userId)
    {
        var invitation = invitationRepo.GetById(invitationId);
        if (invitation == null || invitation.InvitedByUserId != userId)
            throw new InvalidOperationException("Invitation not found or access denied.");
        invitationRepo.Delete(invitationId);
    }



    private static GroupInvitation ToEntity(int userId, int actorUserId, int groupId) => new()
    {
        InvitedUserId = userId,
        GroupId = groupId,
        InvitedByUserId = actorUserId,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(30)
    };

    private static InboxItemDto Map(GroupInvitation invitation, string actorUserName, string groupName) => new()
    {
        Id = invitation.Id,
        Type = "GroupInvitation",
        ContextId = invitation.GroupId,
        ContextName = groupName,
        SentBy = actorUserName,
        CreatedAt = invitation.CreatedAt,
        ExpiresAt = invitation.ExpiresAt
    };

    private static void ValidateUser(CreateGroupInvitationDto dto, Models.User.User? user, Models.User.User? actorUser, Models.Group.Group group)
    {
        if (user == null)            
            throw new InvalidOperationException($"User with email {dto.InviteeEmail} not found.");
        if (actorUser == null)       
            throw new InvalidOperationException("Actor user not found.");
        if (group == null)           
            throw new InvalidOperationException("Group not found or access denied.");
        if (user.Id == actorUser.Id)
            throw new InvalidOperationException("Users cannot invite themselves to a group.");
    }
}