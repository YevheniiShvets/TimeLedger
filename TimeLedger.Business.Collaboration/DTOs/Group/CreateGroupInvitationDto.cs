using System.ComponentModel.DataAnnotations;

namespace BusinessCollaboration.DTOs.Group;

public class CreateGroupInvitationDto
{
    [Required]
    [MaxLength(254)]
    [EmailAddress]
    public string InviteeEmail { get; set; } = string.Empty;
}