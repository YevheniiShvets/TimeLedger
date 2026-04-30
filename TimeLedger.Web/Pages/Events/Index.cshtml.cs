using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Models;
using TimeLedger.Core.Models.Events;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Events;

public class IndexModel : PageModel
{
    private readonly EventService _svc;

    public IndexModel(EventService svc)
    {
        _svc = svc;
    }

    public IEnumerable<EventResponseDto> Events { get; set; } = [];

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        Events = _svc.GetAll(EventOwnerType.User, userId.Value);
        return Page();
    }
}