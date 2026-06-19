using System.ComponentModel.DataAnnotations;

namespace BusinessCollaboration.DTOs.Group;

public class AddMemberDto
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}