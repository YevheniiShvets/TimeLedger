using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.DTOs;
using TimeLedger.Services;

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

    public void OnGet() { }

    public  IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (_, hasOverlap) =  _svc.Create(Input);
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