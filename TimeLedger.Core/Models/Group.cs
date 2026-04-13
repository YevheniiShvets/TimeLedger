namespace TimeLedger.Core.Models;

public class Group
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
}