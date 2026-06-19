namespace BusinessCollaboration.Interfaces.Group;

public interface IGroupRepository
{
    IEnumerable<Models.Group.Group> GetAllGroups(int ownerId);
    Models.Group.Group GetGroupById(int id, int ownerId);
    Models.Group.Group CreateGroup(Models.Group.Group group);
    Models.Group.Group UpdateGroup(Models.Group.Group group);
    void DeleteGroup(int id, int ownerId);
    
    IEnumerable<int> GetGroupMembers(int groupId);
    void AddGroupMember(int groupId, int userId);
    void RemoveGroupMember(int groupId, int userId);
    bool IsMember(int groupId, int userId);
}