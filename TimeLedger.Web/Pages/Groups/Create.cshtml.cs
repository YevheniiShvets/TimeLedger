using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups;

public class CreateModel(GroupService groupService) : PageModel
{
    [BindProperty]
    public CreateGroupDto Input { get; set; } = new();

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        return Page();
    }

    public IActionResult OnPost()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var group = groupService.Create(Input, userId.Value);
            return RedirectToPage("/Groups/Index", new { id = group.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}

