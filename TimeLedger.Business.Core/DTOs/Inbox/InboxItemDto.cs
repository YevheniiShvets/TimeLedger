namespace TimeLedger.Core.DTOs.Inbox;

public class InboxItemDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int ContextId { get; set; }
    public string? ContextName { get; set; }
    public string SentBy { get; set; } = string.Empty;
    public string? NotificationSubType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}