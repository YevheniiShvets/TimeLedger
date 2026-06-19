using BusinessCollaboration.DTOs.User;
using BusinessCollaboration.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace TimeLedger.Pages.Account;

public class EditAccountModel(UserService userService) : PageModel
{
    [BindProperty]
    public UpdateAccountDto Input { get; set; } = new();

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
            var account = userService.GetById(userId.Value);
            Input = new UpdateAccountDto
            {
                Name = account.Name,
                Email = account.Email
            };

            return Page();
        }
        catch (InvalidOperationException)
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Account/Login");
        }
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
            var account = userService.Update(userId.Value, Input);
            HttpContext.Session.SetString(AuthSession.UserEmailKey, account.Email);
            HttpContext.Session.SetString(AuthSession.UserNameKey, account.Name);

            return RedirectToPage("/Account/Info");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
