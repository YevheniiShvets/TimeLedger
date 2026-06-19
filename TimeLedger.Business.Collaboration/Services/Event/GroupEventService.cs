using BusinessCollaboration.DTOs.Event;
using BusinessCollaboration.DTOs.User;
using BusinessCollaboration.Interfaces.Event;
using BusinessCollaboration.Interfaces.Group;
using BusinessCollaboration.Interfaces.User;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace BusinessCollaboration.Services.Event;

public class GroupEventService(IRemoteEventRepository eventRepository, IGroupRepository groupRepository, IUserRepository userRepository) : IGroupEventService
{
    public IEnumerable<GroupEventResponseDto> GetAllForUser(int actorUserId)
    {
        
        var groups = groupRepository.GetAllGroups(actorUserId).ToList(); // Gets all groups for user
        List<TimeLedger.Core.Models.Event.Event> groupsEvents = [];

        foreach (var group in groups) // Saves all events for all groups in list
        {
            groupsEvents.AddRange(eventRepository.GetAll(EventOwnerType.Group, group.Id)); 
        }

        var events = groupsEvents.Select(e => // Maps all events to GroupEventListItemDto
        {
            var group = groups.First(g => g.Id == e.OwnerId);
            return new GroupEventResponseDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Event = Map(e)
            };
        });
        return events;

    }

    public (EventResponseDto dto, bool hasOverlap, IReadOnlyList<AccountInfoDto> overlappingUsers) Create(int groupId, CreateEventDto dto, int actorUserId)
    {
        var group = GetAccessibleGroup(groupId, actorUserId);
        EnsureOwner(group, actorUserId);

        ValidateFields(dto);

        if (dto.EventType != EventType.Deadline && !dto.AllowOverlap)
        {
            var hasGroupOverlap = eventRepository.HasOverlap(dto.StartTime!.Value, dto.EndTime!.Value, null, EventOwnerType.Group, groupId);
            var overlappingUsers = GetOverlappingMembers(dto, group);

            if (hasGroupOverlap || overlappingUsers.Count > 0)
            {
                return (Map(ToEntity(dto, groupId)), true, overlappingUsers);
            }
        }

        var saved = eventRepository.Add(ToEntity(dto, groupId));
        return (Map(saved), false, []);
    }

    private List<AccountInfoDto> GetOverlappingMembers(CreateEventDto dto, Models.Group.Group group)
    {
        var memberIds = groupRepository.GetGroupMembers(group.Id)
            .Append(group.OwnerId)
            .Distinct()
            .ToList();

        var overlappingIds = eventRepository.GetOverlappingOwnerIds(
            dto.StartTime!.Value,
            dto.EndTime!.Value,
            null,
            EventOwnerType.User,
            memberIds);

        return overlappingIds.Select(userRepository.GetById).OfType<Models.User.User>().Select(MapUser).ToList();
    }
    

    private Models.Group.Group GetAccessibleGroup(int groupId, int actorUserId)
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

    private static void EnsureOwner(Models.Group.Group group, int userId)
    {
        if (group.OwnerId != userId)
            throw new InvalidOperationException("Only the group owner can create events for this group");
    }

    private static void ValidateFields(CreateEventDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        if (dto.Title.Trim().Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.");
        if (dto.Description != null && dto.Description.Trim().Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters.");
        if (dto.Location != null && dto.Location.Trim().Length > 300)
            throw new ArgumentException("Location cannot exceed 300 characters.");

        if (dto.EventType == EventType.Deadline)
        {
            if (!dto.DueAt.HasValue)
                throw new ArgumentException("Due date is required for deadline events.");
        }
        else
        {
            ValidateTimeRange(dto.StartTime, dto.EndTime);
        }
    }

    private static void ValidateTimeRange(DateTime? start, DateTime? end)
    {
        if (!start.HasValue || !end.HasValue)
            throw new ArgumentException("Start time and end time are required.");

        if (start >= end)
            throw new ArgumentException("Start time must be before end time.");
    }

    private static EventResponseDto Map(TimeLedger.Core.Models.Event.Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        AllowOverlap = e.AllowOverlap,
        EventType = e.EventType,
        DueAt = e.DueAt,
        RecurrenceFrequency = e.RecurrenceFrequency,
        RecurrenceInterval = e.RecurrenceInterval,
        RecurrenceEndTime = e.RecurrenceEndTime,
        RecurrenceMaxOccurrences = e.RecurrenceMaxOccurrences,
        RecurrenceInfo = RecurrenceInfoFormatter.Generate(e)
    };

    

    private static AccountInfoDto MapUser(Models.User.User user) => new()
    {
        Id = user.Id,
        Name = GetDisplayName(user),
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };

    private static string GetDisplayName(Models.User.User user)
    {
        if (!string.IsNullOrWhiteSpace(user.Name))
            return user.Name;

        var atIndex = user.Email.IndexOf('@');
        return atIndex > 0 ? user.Email[..atIndex] : user.Email;
    }

    private static TimeLedger.Core.Models.Event.Event ToEntity(CreateEventDto dto, int groupId) => new()
    {
        OwnerType = EventOwnerType.Group,
        OwnerId = groupId,
        Title = dto.Title.Trim(),
        Description = dto.Description,
        Location = dto.Location,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        AllowOverlap = dto.AllowOverlap,
        EventType = dto.EventType,
        DueAt = dto.DueAt,
        RecurrenceFrequency = dto.RecurrenceRule?.RecurrenceFrequency,
        RecurrenceInterval = dto.RecurrenceRule?.RecurrenceInterval,
        RecurrenceValue = dto.RecurrenceRule?.RecurrenceValue,
        RecurrenceEndTime = dto.RecurrenceRule?.RecurrenceEndTime,
        RecurrenceMaxOccurrences = dto.RecurrenceRule?.RecurrenceMaxOccurrences
    };
}
