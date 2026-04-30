using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups;

public class Edit(GroupService groupService) : PageModel
{
    
    [BindProperty]
    public UpdateGroupDto Input { get; set; } = new();
    public GroupInfoDto Group { get; private set; } = new();
    public bool IsOwner { get; private set; }
    
    public IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        return !userId.HasValue ? RedirectToPage("/Account/Login") : LoadPage(id, userId.Value);
    }
    
    public IActionResult OnPostUpdate(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");
        
        if (!ModelState.IsValid)
            return LoadPage(id, userId.Value);

        try
        {
            groupService.Update(id, Input, userId.Value);
            return RedirectToPage(new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, userId.Value);
        }
    }



private IActionResult LoadPage(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        return !userId.HasValue ? RedirectToPage("/Account/Login") : LoadPage(id, userId.Value);
    }

    private IActionResult LoadPage(int id, int userId)
    {
        try
        {
            Group = groupService.GetById(id, userId);
            IsOwner = Group.OwnerId == userId;
            Input = new UpdateGroupDto { Name = Group.Name };
            return Page();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}