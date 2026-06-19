using BusinessCollaboration.DTOs.User;
using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace TimeLedger.Pages.Account;

public class InfoModel(UserService userService) : PageModel
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
