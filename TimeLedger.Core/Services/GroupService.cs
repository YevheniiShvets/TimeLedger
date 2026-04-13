using TimeLedger.Core.DTOs;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Core.Services;

public class GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
{
    public IEnumerable<GroupInfoDto> GetAll(int actorUserId)
        => groupRepository.GetAllGroups(actorUserId).Select(Map);

    public GroupInfoDto GetById(int id, int actorUserId)
    {
        var group = GetExistingGroup(id, actorUserId);
        var members = GetMembers(group.Id, actorUserId).ToList();
        return Map(group, members);
    }

    public GroupInfoDto Create(CreateGroupDto dto, int actorUserId)
    {
        ValidateName(dto.Name);

        var group = groupRepository.CreateGroup(ToEntity(dto, actorUserId));
        return Map(group);
    }

    public GroupInfoDto Update(int id, UpdateGroupDto dto, int actorUserId)
    {
        ValidateName(dto.Name);

        var group = GetExistingGroup(id, actorUserId);
        group.Name = dto.Name.Trim();

        var updatedGroup = groupRepository.UpdateGroup(group);
        var members = GetMembers(updatedGroup.Id, actorUserId).ToList();
        return Map(updatedGroup, members);
    }

    public void Delete(int id, int actorUserId)
    {
        GetExistingGroup(id, actorUserId);
        groupRepository.DeleteGroup(id, actorUserId);
    }

    public void AddMember(int groupId, AddMemberDto dto, int actorUserId)
    {
        ValidateUserId(dto.UserId);

        GetExistingGroup(groupId, actorUserId);
        var user = GetExistingUser(dto.UserId);

        if (groupRepository.IsMember(groupId, user.Id))
            throw new InvalidOperationException("User is already a member of this group");

        groupRepository.AddGroupMember(groupId, user.Id);
    }

    public void RemoveMember(int groupId, int userId, int actorUserId)
    {
        ValidateUserId(userId);

        GetExistingGroup(groupId, actorUserId);
        GetExistingUser(userId);

        if (!groupRepository.IsMember(groupId, userId))
            throw new InvalidOperationException("User is not a member of this group");

        groupRepository.RemoveGroupMember(groupId, userId);
    }

    public IEnumerable<AccountInfoDto> GetMembers(int groupId, int actorUserId)
    {
        GetExistingGroup(groupId, actorUserId);

        return groupRepository.GetGroupMembers(groupId)
            .Select(GetExistingUser)
            .Select(MapUser);
    }

    // Private helpers
    
    
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Group name is required");

        if (name.Trim().Length > 100)
            throw new InvalidOperationException("Group name must be 100 characters or less");
    }

    private static void ValidateUserId(int userId)
    {
        if (userId < 1)
            throw new InvalidOperationException("User is required");
    }

    private Group GetExistingGroup(int id, int actorUserId)
    {
        try
        {
            return groupRepository.GetGroupById(id, actorUserId);
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException("Group not found or access denied");
        }
    }

    private User GetExistingUser(int id)
    {
        var user = userRepository.GetById(id);
        return user ?? throw new InvalidOperationException("User not found");
    }

    private static Group ToEntity(CreateGroupDto dto, int actorUserId)
    {
        return new Group
        {
            OwnerId = actorUserId,
            Name = dto.Name.Trim()
        };
    }

    private static GroupInfoDto Map(Group group)
    {
        return new GroupInfoDto
        {
            Id = group.Id,
            Name = group.Name,
            Members = []
        };
    }

    private static GroupInfoDto Map(Group group, List<AccountInfoDto> members)
    {
        return new GroupInfoDto
        {
            Id = group.Id,
            Name = group.Name,
            Members = members
        };
    }

    private static AccountInfoDto MapUser(User user) => new()
    {
        Id = user.Id,
        Name = string.IsNullOrWhiteSpace(user.Name) ? GetDisplayName(user) : user.Name,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };

    private static string GetDisplayName(User user)
    {
        var atIndex = user.Email.IndexOf('@');
        return atIndex > 0 ? user.Email[..atIndex] : user.Email;
    }
}