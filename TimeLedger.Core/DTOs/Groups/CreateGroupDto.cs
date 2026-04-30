using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs.Groups;

public class CreateGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}