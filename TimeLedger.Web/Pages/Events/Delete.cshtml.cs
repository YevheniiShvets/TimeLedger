using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
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

    public  IActionResult OnGet(int id)
    {
        var ev =  _svc.GetById(id);
        if (ev is null)
            return NotFound();
        Event = ev;
        return Page();
    }

    public  IActionResult OnPost(int id)
    {
        try
        {
             _svc.Delete(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return RedirectToPage("Index");
    }
}