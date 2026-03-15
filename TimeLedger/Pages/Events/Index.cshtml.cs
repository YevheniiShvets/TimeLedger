using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.DTOs;
using TimeLedger.Services;

namespace TimeLedger.Pages.Events;

public class IndexModel : PageModel
{
    private readonly IEventService _svc;

    public IndexModel(IEventService svc)
    {
        _svc = svc;
    }

    public IEnumerable<EventResponseDto> Events { get; set; } = [];

    public async Task OnGetAsync()
    {
        Events = await _svc.GetAllAsync();
    }
}