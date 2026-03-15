using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.DTOs;
using TimeLedger.Services;

namespace TimeLedger.Pages.Events;

public class DeleteModel : PageModel
{
    private readonly IEventService _svc;

    public DeleteModel(IEventService svc)
    {
        _svc = svc;
    }

    public EventResponseDto Event { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await _svc.GetByIdAsync(id);
        if (ev is null)
            return NotFound();
        Event = ev;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _svc.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return RedirectToPage("Index");
    }
}