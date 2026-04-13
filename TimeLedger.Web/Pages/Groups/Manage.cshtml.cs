using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeLedger.Core.DTOs;
using TimeLedger.Core.Services;

namespace TimeLedger.Pages.Groups;

public class ManageModel(GroupService groupService) : PageModel
{
    [BindProperty]
    public UpdateGroupDto UpdateInput { get; set; } = new();

    [BindProperty]
    public AddMemberDto AddMemberInput { get; set; } = new();

    public GroupInfoDto Group { get; private set; } = new();

    public IActionResult OnGet(int id)
    {
        var userId = GetSignedInUserId();
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        return LoadPage(id, userId.Value);
    }

    public IActionResult OnPostUpdate(int id)
    {
        var userId = GetSignedInUserId();
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        ModelState.Remove(nameof(AddMemberInput.UserId));
        if (!ModelState.IsValid)
            return LoadPage(id, userId.Value);

        try
        {
            groupService.Update(id, UpdateInput, userId.Value);
            return RedirectToPage(new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, userId.Value);
        }
    }

    public IActionResult OnPostAddMember(int id)
    {
        var userId = GetSignedInUserId();
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        ModelState.Remove("UpdateInput.Name");
        if (!TryValidateModel(AddMemberInput, nameof(AddMemberInput)))
            return LoadPage(id, userId.Value);

        try
        {
            groupService.AddMember(id, AddMemberInput, userId.Value);
            return RedirectToPage(new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return LoadPage(id, userId.Value);
        }
    }

    public IActionResult OnPostRemoveMember(int id, int userId)
    {
        var actorUserId = GetSignedInUserId();
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

    public IActionResult OnPostDelete(int id)
    {
        var userId = GetSignedInUserId();
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

    private int? GetSignedInUserId()
    {
        return HttpContext.Session.GetInt32(AuthSession.UserIdKey);
    }

    private IActionResult LoadPage(int id)
    {
        var userId = GetSignedInUserId();
        if (!userId.HasValue)
            return RedirectToPage("/Account/Login");

        return LoadPage(id, userId.Value);
    }

    private IActionResult LoadPage(int id, int actorUserId)
    {
        try
        {
            Group = groupService.GetById(id, actorUserId);
            UpdateInput = new UpdateGroupDto { Name = Group.Name };
            return Page();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}

