using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs.Inbox;
using TimeLedger.Core.Interfaces.Groups;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Inbox;

public class Index(IGroupInvitationService invitationService) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public IEnumerable<InboxItemDto> InboxItems { get; private set; } = [];
    
    public void OnGet()
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
        {
            Response.Redirect("/Account/Login");
            return;
        }

        InboxItems = invitationService.GetPendingInboxForUser(userId.Value);
    }

    public IActionResult OnPostAcceptInvitation(int invitationId)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
            invitationService.AcceptInvitation(invitationId, userId.Value);
            StatusMessage = "Invitation accepted successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to accept invitation: {ex.Message}";
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDeclineInvitation(int invitationId)
    {
        var userId = HttpContext.Session.GetInt32(AuthSession.UserIdKey);
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        try
        {
            invitationService.DeclineInvitation(invitationId, userId.Value);
            StatusMessage = "Invitation declined.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to decline invitation: {ex.Message}";
        }

        return RedirectToPage();
    }
}