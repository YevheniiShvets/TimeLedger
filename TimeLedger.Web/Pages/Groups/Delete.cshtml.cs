using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups;

public class Delete (GroupService groupService) : PageModel
{
    
    public GroupInfoDto Group { get; private set; } = new();
    public bool IsOwner { get; private set; }
    
    public IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        return !userId.HasValue ? RedirectToPage("/Account/Login") : LoadPage(id, userId.Value);
    }
    
    public IActionResult OnPost(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
            groupService.Delete(id, userId.Value);
            return RedirectToPage("/Groups/Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, userId.Value);
        }
    }
    
    
    private IActionResult LoadPage(int id, int userId)
    {
        try
        {
            Group = groupService.GetById(id, userId);
            IsOwner = Group.OwnerId == userId;
            return Page();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}