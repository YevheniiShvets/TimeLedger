using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.Services;


namespace TimeLedger.Pages.Account;

public class RegisterModel(UserService userService) : PageModel
{
    [BindProperty]
    public RegisterDto Input { get; set; } = new();

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
            var account = userService.Register(Input);
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
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}

