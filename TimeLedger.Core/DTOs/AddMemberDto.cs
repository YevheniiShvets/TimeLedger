using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs;

public class AddMemberDto
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;
}