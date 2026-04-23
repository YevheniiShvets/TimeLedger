using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups;

public class IndexModel(GroupService groupService) : PageModel
{
    public IEnumerable<GroupInfoDto> Groups { get; private set; } = [];
    public int CurrentUserId { get; private set; }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        CurrentUserId = userId.Value;
        Groups = groupService.GetAll(userId.Value);
        return Page();
    }
}

