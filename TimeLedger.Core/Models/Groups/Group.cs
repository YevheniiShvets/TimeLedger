namespace TimeLedger.Core.Models.Groups;

public class Group
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
}