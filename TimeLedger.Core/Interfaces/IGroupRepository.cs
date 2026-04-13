using TimeLedger.Core.Models;

namespace TimeLedger.Core.Interfaces;

public interface IGroupRepository
{
    IEnumerable<Group> GetAllGroups(int ownerId);
    Group GetGroupById(int id, int ownerId);
    Group CreateGroup(Group group);
    Group UpdateGroup(Group group);
    void DeleteGroup(int id, int ownerId);
    
    IEnumerable<int> GetGroupMembers(int groupId);
    void AddGroupMember(int groupId, int userId);
    void RemoveGroupMember(int groupId, int userId);
    bool IsMember(int groupId, int userId);
}