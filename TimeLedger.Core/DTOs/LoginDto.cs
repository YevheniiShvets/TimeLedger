using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs;

public class LoginDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    [PasswordPropertyText]
    public string Password { get; set; } = string.Empty;
}