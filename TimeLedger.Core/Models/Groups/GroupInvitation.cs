namespace TimeLedger.Core.Models.Groups;

public class GroupInvitation
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int InvitedUserId { get; set; }
    public int InvitedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}