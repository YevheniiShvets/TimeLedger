using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs;

public class UpdateGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}