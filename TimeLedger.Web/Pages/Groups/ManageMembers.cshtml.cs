using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.Interfaces.Groups;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups;

public class ManageModel(GroupService groupService, IGroupInvitationService invitationService) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    

    [BindProperty]
    public CreateGroupInvitationDto AddMemberInput { get; set; } = new();
    public GroupInfoDto Group { get; private set; } = new();
    public bool IsOwner { get; private set; }

    public IActionResult OnGet(int id)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        return !userId.HasValue ? RedirectToPage("/Account/Login") : LoadPage(id, userId.Value);
    }

    

    public IActionResult OnPostAddMember(int id, [FromForm(Name = "AddMemberInput.InviteeEmail")] string email)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");
        
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Email is required.");
            return LoadPage(id, userId.Value);
        }

        AddMemberInput.InviteeEmail = email.Trim();

        try
        {
            invitationService.Invite(id, AddMemberInput, userId.Value);
            StatusMessage = "Invitation sent";
            return RedirectToPage(new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, userId.Value);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, userId.Value);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Failed to sent invitation: {ex.Message}");
            return LoadPage(id, userId.Value);
        }
    }

    public IActionResult OnPostRemoveMember(int id, int userId)
    {
        var actorUserId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!actorUserId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
            groupService.RemoveMember(id, userId, actorUserId.Value);
            return RedirectToPage(new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, actorUserId.Value);
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
            return Page();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
