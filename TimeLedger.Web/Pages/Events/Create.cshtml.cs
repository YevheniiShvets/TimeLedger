using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Models;
using TimeLedger.Core.Models.Events;
using TimeLedger.Core.Services;


namespace TimeLedger.Pages.Events;

public class CreateModel : PageModel
{
    private readonly EventService _svc;

    public CreateModel(EventService svc)
    {
        _svc = svc;
    }

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

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (_, hasOverlap) =  _svc.Create(Input, EventOwnerType.User, userId.Value);
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