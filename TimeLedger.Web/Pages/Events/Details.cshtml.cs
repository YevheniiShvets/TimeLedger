using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Events;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Events;

public class DetailsModel(IEventService svc) : PageModel
{
    public EventResponseDto Event { get; set; } = null!;

    public IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var ev = svc.GetById(id, EventOwnerType.User, userId.Value);
        if (ev is null)
            return NotFound();

        Event = ev;
        return Page();
    }
}

