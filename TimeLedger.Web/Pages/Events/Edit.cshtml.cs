using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Events;

public class EditModel : PageModel
{
    private readonly EventService _svc;

    public EditModel(EventService svc)
    {
        _svc = svc;
    }

    [BindProperty]
    public UpdateEventDto Input { get; set; } = new();

    [BindProperty]
    public int EventId { get; set; }

    public bool ShowOverlapWarning { get; set; }

    public  IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var ev =  _svc.GetById(id, userId.Value);
        if (ev is null)
            return NotFound();

        EventId = id;
        Input = new UpdateEventDto
        {
            Title        = ev.Title,
            Description  = ev.Description,
            Location     = ev.Location,
            StartTime    = ev.StartTime,
            EndTime      = ev.EndTime,
            AllowOverlap = ev.AllowOverlap,
        };
        return Page();
    }

    public  IActionResult OnPost()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (_, hasOverlap) =  _svc.Update(EventId, Input, userId.Value);
            if (hasOverlap)
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