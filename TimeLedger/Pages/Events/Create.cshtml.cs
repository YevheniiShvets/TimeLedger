using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.DTOs;
using TimeLedger.Services;

namespace TimeLedger.Pages.Events;

public class CreateModel : PageModel
{
    private readonly IEventService _svc;

    public CreateModel(IEventService svc)
    {
        _svc = svc;
    }

    [BindProperty]
    public CreateEventDto Input { get; set; } = new();

    public bool ShowOverlapWarning { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (_, hasOverlap) = await _svc.CreateAsync(Input);
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