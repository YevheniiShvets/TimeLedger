using TimeLedger.Core.DTOs;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Core.Services;

public class GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
{
    public IEnumerable<GroupInfoDto> GetAll(int actorUserId)
        => groupRepository.GetAllGroups(actorUserId).Select(Map);

    public GroupInfoDto GetById(int groupId, int userId)
    {
        var group = GetAccessibleGroup(groupId, userId);
        var members = groupRepository.GetGroupMembers(group.Id)
            .Append(group.OwnerId)
            .Distinct()
            .Select(id => userRepository.GetById(id) ?? throw new InvalidOperationException($"User {id} not found"))
            .Select(MapUser).ToList();
        return Map(group, members);
    }

    public GroupInfoDto Create(CreateGroupDto dto, int userId)
    {
        ValidateForm(dto.Name);

        var group = groupRepository.CreateGroup(ToEntity(dto, userId));
        return Map(group);
    }

    public GroupInfoDto Update(int groupId, UpdateGroupDto dto, int userId)
    {
        var group = GetAccessibleGroup(groupId, userId);
        EnsureOwner(group, userId);

        ValidateForm(dto.Name);

        group.Name = dto.Name.Trim();
        var updatedGroup = groupRepository.UpdateGroup(group);
        var members = groupRepository.GetGroupMembers(updatedGroup.Id)
            .Append(updatedGroup.OwnerId)
            .Distinct()
            .Select(id => userRepository.GetById(id) ?? throw new InvalidOperationException($"User {id} not found"))
            .Select(MapUser).ToList();
        return Map(updatedGroup, members);
    }

    public void Delete(int id, int userId)
    {
        var group = GetAccessibleGroup(id, userId);
        EnsureOwner(group, userId);
        groupRepository.DeleteGroup(group.Id, userId);
    }

    public void AddMember(int groupId, AddMemberDto dto, int userId)
    {
        ValidateEmail(dto.Email);

        var group = GetAccessibleGroup(groupId, userId);
        EnsureOwner(group, userId);
        var user = userRepository.GetByEmail(dto.Email.Trim()) ?? throw new InvalidOperationException("User not found");

        if (user.Id == group.OwnerId || groupRepository.IsMember(groupId, user.Id))
            throw new InvalidOperationException("User is already a member of this group");

        groupRepository.AddGroupMember(groupId, user.Id);
    }

    public void RemoveMember(int groupId, int userId, int actorUserId)
    {
        if (userId < 1)
            throw new ArgumentException("User is required.");

        var group = GetAccessibleGroup(groupId, actorUserId);
        EnsureOwner(group, actorUserId);

        if (userId == group.OwnerId)
            throw new InvalidOperationException("Owner cannot be removed from the group");

        

        if (!groupRepository.IsMember(groupId, userId))
            throw new InvalidOperationException("User is not a member of this group");

        groupRepository.RemoveGroupMember(groupId, userId);
    }

    public IEnumerable<AccountInfoDto> GetMembers(int groupId, int userId)
    {
        var group = GetAccessibleGroup(groupId, userId);
        return groupRepository.GetGroupMembers(group.Id)
            .Append(group.OwnerId)
            .Distinct()
            .Select(id => userRepository.GetById(id) ?? throw new InvalidOperationException($"User {id} not found"))
            .Select(MapUser);
    }


// Private helpers
    
    
    
    private Group GetAccessibleGroup(int groupId, int actorUserId)
    {
        try
        {
            return groupRepository.GetGroupById(groupId, actorUserId)
                ?? throw new InvalidOperationException("Group not found or access denied");
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException("Group not found or access denied");
        }
    }

    private static void ValidateForm(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required.");
        if (name.Trim().Length > 100)
            throw new ArgumentException("Group name cannot exceed 100 characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        if (email.Trim().Length > 254)
            throw new ArgumentException("Email cannot exceed 254 characters.");
    }

    private static void EnsureOwner(Group group, int userId)
    {
        if (group.OwnerId != userId)
            throw new InvalidOperationException("Only the group owner can modify this group");
    }

    private static Group ToEntity(CreateGroupDto dto, int userId) => new()
    {
        OwnerId = userId,
        Name = dto.Name.Trim()
    };

    private static GroupInfoDto Map(Group group) => new()
    {
        Id = group.Id,
        OwnerId = group.OwnerId,
        Name = group.Name,
        Members = []
    };

    private static GroupInfoDto Map(Group group, List<AccountInfoDto> members) => new()
    {
        Id = group.Id,
        OwnerId = group.OwnerId,
        Name = group.Name,
        Members = members
    };

    private static AccountInfoDto MapUser(User user) => new()
    {
        Id = user.Id,
        Name = GetDisplayName(user),
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };

    private static string GetDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.Name))
            return user.Name;

        var atIndex = user.Email.IndexOf('@');
        return atIndex > 0 ? user.Email[..atIndex] : user.Email;
    }
}