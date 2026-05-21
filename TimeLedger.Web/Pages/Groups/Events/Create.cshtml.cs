using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Interfaces.Groups;
using TimeLedger.Core.Models.Events;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups.Events;

public class CreateModel(IGroupEventService groupEventService, IGroupService groupService) : PageModel
{
    [BindProperty]
    public CreateEventDto Input { get; set; } = new();

    public string GroupName { get; private set; } = string.Empty;
    public bool ShowOverlapWarning { get; set; }
    public List<string> OverlappingUsers { get; private set; } = [];

    public IActionResult OnGet(int groupId)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var group = groupService.GetById(groupId, userId.Value);
        if (group.OwnerId != userId.Value)
            return Forbid();

        GroupName = group.Name;
        return Page();
    }

    public IActionResult OnPost(int groupId)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var group = groupService.GetById(groupId, userId.Value);
        if (group.OwnerId != userId.Value)
            return Forbid();

        GroupName = group.Name;

        if (Input.EventType == EventType.Deadline)
        {
            ModelState.Remove("Input.StartTime");
            ModelState.Remove("Input.EndTime");
            Input.StartTime = null;
            Input.EndTime = null;
        }
        else
        {
            ModelState.Remove("Input.DueAt");
            Input.DueAt = null;
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (_, hasOverlap, overlappingUsers) = groupEventService.Create(groupId, Input, userId.Value);
            if (!hasOverlap)
                return RedirectToPage("/Groups/Events/Index");

            ShowOverlapWarning = true;
            OverlappingUsers = overlappingUsers.Select(u => u.Name).ToList();
            return Page();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
