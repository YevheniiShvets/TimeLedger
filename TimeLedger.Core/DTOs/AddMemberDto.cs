using System.ComponentModel.DataAnnotations;

namespace TimeLedger.Core.DTOs;

public class AddMemberDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}