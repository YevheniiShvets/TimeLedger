using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs.Groups;

public class UpdateGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}