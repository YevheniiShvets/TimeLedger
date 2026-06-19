using BusinessCollaboration.Interfaces.Event;
using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Pages.Groups.Events;

public class IndexModel(IGroupEventService groupEventService) : PageModel
{
    public IReadOnlyList<GroupEventListItemViewModel> Events { get; private set; } = [];

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        Events = groupEventService
            .GetAllForUser(userId.Value)
            .Select(item => new GroupEventListItemViewModel
            {
                GroupId = item.GroupId,
                GroupName = item.GroupName,
                Event = item.Event,
                SortTime = GetSortTime(item.Event)
            })
            .OrderBy(item => item.SortTime)
            .ToList();

        return Page();
    }

    private static DateTime GetSortTime(EventResponseDto e)
    {
        if (e.EventType == EventType.Deadline)
            return e.DueAt ?? DateTime.MaxValue;

        return e.StartTime ?? DateTime.MaxValue;
    }

    public class GroupEventListItemViewModel
    {
        public int GroupId { get; init; }
        public string GroupName { get; init; } = string.Empty;
        public EventResponseDto Event { get; init; } = new();
        public DateTime SortTime { get; init; }
    }
}

