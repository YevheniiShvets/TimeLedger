namespace BusinessCollaboration.DTOs.Synchronisation;

public class SyncResultDto
{
    public int PushedCount { get; set; }
    public int PulledCount { get; set; }
    public DateTime SyncedAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    public bool HasError => ErrorMessage is not null;
 
    public static SyncResultDto Failure(string error) => new()
    {
        ErrorMessage = error,
        SyncedAt = DateTime.UtcNow
    };

}