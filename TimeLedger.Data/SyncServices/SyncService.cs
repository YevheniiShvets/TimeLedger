using BusinessCollaboration.DTOs.Synchronisation;
using BusinessCollaboration.Interfaces.Event;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Interfaces.Settings;
using TimeLedger.Core.Interfaces.Synchronisation;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Infrastructure.SyncServices;

public class SyncService(IRemoteEventRepository remote, IEventRepository local, ISettingsStore settings, IConnectivityService connectivity, ISyncStateStore syncState)
{
        private const string SyncModeKey = "sync_mode";
        
    
    public void SyncOnStartupIfEnabled(int userId)
    {
        var mode = settings.Get<SyncMode>(SyncModeKey);
        if (mode == SyncMode.Auto)
            Sync(userId);
    }
    
    public SyncResultDto Sync(int userId)
    {
        if (userId <= 0)
            return SyncResultDto.Failure("Cannot sync an offline-only session.");

        if (!connectivity.IsAvailable())
            return SyncResultDto.Failure("Server is not reachable.");

        try
        {
            var pushedCount = Push(userId);
            var pulledCount = Pull(userId);

            var syncedAt = DateTime.UtcNow;
            syncState.SetLastSyncedAt(syncedAt);

            return new SyncResultDto
            {
                PushedCount = pushedCount,
                PulledCount = pulledCount,
                SyncedAt = syncedAt
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== SYNC EXCEPTION ===");
            Console.WriteLine($"Type: {ex.GetType().FullName}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");

            return SyncResultDto.Failure(ex.Message);
        }
    }
 
    // -------------------------------------------------------------------------
    // Private phases
    // -------------------------------------------------------------------------
    
    private int Push(int userId)
    {
        var lastSync = syncState.GetLastSyncedAt() ?? DateTime.MinValue;
        Console.WriteLine($"Last sync was at {lastSync:o}");
        var pending = local.GetUpdatedSince(lastSync, EventOwnerType.User, userId).ToList();
        foreach (var e in pending)
        {
            Console.WriteLine($"{e.Id} - {e.Title} - {e.UpdatedAt:o}");
        }

        foreach (var e in pending)
        {
            try
            {
                if (e.IsDeleted)
                {
                    if (e.Id > 0)
                        remote.Delete(e);
                    local.Delete(e);
                    continue;
                }

                if (e.Id < 0)
                {
                    Console.WriteLine($"Pushing new event (temp Id={e.Id}): {e.Title}");
                    var oldLocalId = e.Id;
                    Console.WriteLine($"Adding to remote...");
                    e.Id = 0;
                    e.OwnerId = userId;
                    Console.WriteLine($"Pushing event (temp Id={e.Id})");
                    var remoteSaved = remote.Add(e);
                    Console.WriteLine($"Remote assigned Id={remoteSaved.Id}");
                    local.Delete(new Event { Id = oldLocalId });
                    Console.WriteLine($"Deleted local event with temp Id={oldLocalId}");
                    local.Upsert(remoteSaved);
                    Console.WriteLine($"Assigned new Id={remoteSaved.Id} for event '{e.Title}'");
                }
                else
                {
                    remote.Upsert(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Push failed for event Id={e.Id}, Title={e.Title}: {ex}");
                throw;
            }
        }

        return pending.Count;
    }
    
    private int Pull(int userId)
    {
        var lastSync = syncState.GetLastSyncedAt() ?? DateTime.MinValue;
 
        var changed = remote
            .GetUpdatedSince(lastSync, EventOwnerType.User, userId)
            .ToList();
 
        foreach (var e in changed)
            local.Upsert(e);
 
        return changed.Count;
    }

}