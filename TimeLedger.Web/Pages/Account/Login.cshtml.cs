using BusinessCollaboration.DTOs.User;
using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace TimeLedger.Pages.Account;

public class LoginModel(UserService userService) : PageModel
{
    [BindProperty]
    public LoginDto Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetInt32(AuthSession.UserIdKey).HasValue)
            return RedirectToPage("/Events/Index");

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            AccountInfoDto account = userService.Login(Input);
            HttpContext.Session.SetInt32(AuthSession.UserIdKey, account.Id);
            HttpContext.Session.SetString(AuthSession.UserEmailKey, account.Email);
            HttpContext.Session.SetString(AuthSession.UserNameKey, account.Name);

            return RedirectToPage("/Events/Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
