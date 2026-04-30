using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Events;
using TimeLedger.Core.Models;
using TimeLedger.Core.Models.Events;
using TimeLedger.Core.Services;


namespace TimeLedger.Pages.Events;

public class DeleteModel : PageModel
{
    private readonly EventService _svc;

    public DeleteModel(EventService svc)
    {
        _svc = svc;
    }

    public EventResponseDto Event { get; set; } = null!;

    public IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        var ev =  _svc.GetById(id, EventOwnerType.User, userId.Value);
        if (ev is null)
            return NotFound();
        Event = ev;
        return Page();
    }

    public  IActionResult OnPost(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
             _svc.Delete(id, EventOwnerType.User, userId.Value);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return RedirectToPage("Index");
    }
}