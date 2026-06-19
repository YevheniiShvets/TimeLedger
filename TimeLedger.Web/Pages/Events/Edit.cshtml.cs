using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace TimeLedger.Pages.Events;

public class EditModel(EventService svc) : PageModel
{
    [BindProperty]
    public UpdateEventDto Input { get; set; } = new();

    [BindProperty]
    public int EventId { get; set; }

    public bool ShowOverlapWarning { get; set; }

    public IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var ev = svc.GetById(id, EventOwnerType.User, userId.Value);
        if (ev is null)
            return NotFound();

        EventId = id;
        if (ev.RecurrenceInterval != null)
            Input = new UpdateEventDto
            {
                Title = ev.Title,
                Description = ev.Description,
                Location = ev.Location,
                EventType = ev.EventType,
                StartTime = ev.StartTime,
                EndTime = ev.EndTime,
                DueAt = ev.DueAt,
                AllowOverlap = ev.AllowOverlap,
                RecurrenceRule = ev.EventType == EventType.Recurrence && ev.RecurrenceFrequency.HasValue
                    ? new RecurrenceRuleDto
                    {
                        RecurrenceFrequency = ev.RecurrenceFrequency.Value,
                        RecurrenceInterval = ev.RecurrenceInterval.Value,
                        RecurrenceEndTime = ev.RecurrenceEndTime,
                        RecurrenceMaxOccurrences = ev.RecurrenceMaxOccurrences
                    }
                    : null
            };
        return Page();
    }

    public IActionResult OnPost()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        if (Input.EventType == EventType.Deadline)
        {
            ModelState.Remove("Input.StartTime");
            ModelState.Remove("Input.EndTime");
            Input.StartTime = null;
            Input.EndTime = null;
        }
        else if (Input.EventType == EventType.OneTime)
        {
            ModelState.Remove("Input.DueAt");
            Input.DueAt = null;
        }

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (_, hasOverlap) = svc.Update(EventId, Input, EventOwnerType.User, userId.Value);
            if (hasOverlap && Input.EventType != EventType.Deadline)
            {
                ShowOverlapWarning = true;
                return Page();
            }
            return RedirectToPage("Index");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}