using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;

namespace TimeLedger.Pages.Events;

public class IndexModel(EventService eventService, EventOccurrenceService occurrenceService) : PageModel
{
    public IEnumerable<EventResponseDto> Events { get; set; } = [];

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var ownerType = EventOwnerType.User;
        var ownerId = userId.Value;
        
        var rangeStart = DateTime.Today.AddMonths(-3);
        var rangeEnd = DateTime.Today.AddMonths(3);

        var recurrenceEvents = occurrenceService.GetOccurrencesInRange(ownerType, ownerId, rangeStart, rangeEnd).ToList();
        var oneTimeAndDeadlineEvents = eventService.GetAll(ownerType, ownerId)
            .Where(e => e.EventType == EventType.OneTime || e.EventType == EventType.Deadline)
            .Select(e => new EventResponseDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Location = e.Location,
                StartTime = e.StartTime ?? e.DueAt.Value,
                EndTime = e.EndTime ?? e.DueAt.Value.AddSeconds(1),
                OwnerType = e.OwnerType,
                OwnerId = e.OwnerId,
                EventType = e.EventType,
                RecurrenceInfo = e.RecurrenceInfo

            });
        Events = recurrenceEvents.Concat(oneTimeAndDeadlineEvents)
            .OrderBy(e => e.StartTime)
            .ThenBy(e => e.EndTime)
            .ToList();
        

        return Page();
    }
}