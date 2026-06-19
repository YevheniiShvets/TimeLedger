using System.ComponentModel.DataAnnotations;

namespace BusinessCollaboration.DTOs.Group;

public class CreateGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}