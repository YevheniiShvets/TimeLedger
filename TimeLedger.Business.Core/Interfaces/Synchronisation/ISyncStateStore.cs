namespace TimeLedger.Core.Interfaces.Synchronisation;

public interface ISyncStateStore
{
    public DateTime? GetLastSyncedAt();
    public void SetLastSyncedAt(DateTime at);
}