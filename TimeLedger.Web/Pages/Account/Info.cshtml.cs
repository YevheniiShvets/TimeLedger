using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Users;
using TimeLedger.Core.Interfaces.Users;
using TimeLedger.Core.Services;


namespace TimeLedger.Pages.Account;

public class InfoModel(IUserService userService) : PageModel
{
    public AccountInfoDto Account { get; private set; } = new();

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
            Account = userService.GetById(userId.Value);
            return Page();
        }
        catch (InvalidOperationException)
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Account/Login");
        }
    }
}
