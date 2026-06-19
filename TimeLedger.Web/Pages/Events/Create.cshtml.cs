using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Event;
using TimeLedger.Core.Models.Event;
using TimeLedger.Core.Services.Event;


namespace TimeLedger.Pages.Events;

public class CreateModel(EventService svc) : PageModel
{
    [BindProperty]
    public CreateEventDto Input { get; set; } = new();

    public bool ShowOverlapWarning { get; set; }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

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
            var (_, hasOverlap) =  svc.Create(Input, EventOwnerType.User, userId.Value);
            if (!hasOverlap) return RedirectToPage("Index");
            ShowOverlapWarning = true;
            return Page();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}