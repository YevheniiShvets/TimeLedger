namespace TimeLedger.Core.DTOs;

public class GroupInfoDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<AccountInfoDto> Members { get; set; } = [];
}