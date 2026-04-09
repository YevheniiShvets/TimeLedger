using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.DTOs;
using TimeLedger.Services;

namespace TimeLedger.Pages.Events;

public class IndexModel : PageModel
{
    private readonly EventService _svc;

    public IndexModel(EventService svc)
    {
        _svc = svc;
    }

    public IEnumerable<EventResponseDto> Events { get; set; } = [];

    public void OnGet()
    {
        Events = _svc.GetAll();
    }
}