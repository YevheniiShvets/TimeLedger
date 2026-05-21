using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.DTOs.Users;

namespace TimeLedger.Core.Interfaces.Groups;

public interface IGroupService
{
    IEnumerable<GroupInfoDto> GetAll(int actorUserId);
    GroupInfoDto GetById(int groupId, int userId);
    GroupInfoDto Create(CreateGroupDto dto, int userId);
    GroupInfoDto Update(int groupId, UpdateGroupDto dto, int userId);
    void Delete(int id, int userId);
    void RemoveMember(int groupId, int userId, int actorUserId);
    IEnumerable<AccountInfoDto> GetMembers(int groupId, int userId);
}

