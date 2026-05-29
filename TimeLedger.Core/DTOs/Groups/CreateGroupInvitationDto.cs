using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs.Groups;

public class CreateGroupInvitationDto
{
    [Required]
    [MaxLength(254)]
    [EmailAddress]
    public string InviteeEmail { get; set; } = string.Empty;
}