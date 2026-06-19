using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Interfaces.Settings;
using TimeLedger.Core.Interfaces.Synchronisation;
using TimeLedger.Core.Models.Event;
using TimeLedger.Infrastructure.SyncServices;

namespace TimeLedger.Infrastructure.Repositories;

public class SyncedEventRepository(IEventRepository remote, IEventRepository local, IConnectivityService connectivity, ISettingsStore settings) : IEventRepository
{
    
    // -------------------------------------------------------------------------
    // Reads — remote first, local fallback
    // -------------------------------------------------------------------------
 
    public IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId)
    {
        if (connectivity.IsAvailable())
        {
            var events = remote.GetAll(ownerType, ownerId).ToList();
            events.AddRange(local.GetAll(ownerType, ownerId).ExceptBy(events.Select(e => e.Id), e => e.Id));
            return events;
        }
        return local.GetAll(ownerType, ownerId);
    }
 
    public Event? GetById(int id, EventOwnerType ownerType, int ownerId)
    {
        if (connectivity.IsAvailable())
        {
            var Event = local.GetById(id, ownerType, ownerId) ?? remote.GetById(id, ownerType, ownerId);
            return Event;
        }
            
 
        return local.GetById(id, ownerType, ownerId);
    }
 
    // -------------------------------------------------------------------------
    // Writes — remote first when online, local only when offline
    // -------------------------------------------------------------------------
 
    private bool ShouldPushImmediately(Event e) =>
        e.OwnerId > 0 && connectivity.IsAvailable() && settings.Get<SyncMode>("sync_mode") == SyncMode.Auto;

    public Event Add(Event e)
    {
        e.UpdatedAt = DateTime.UtcNow;
        var saved = local.Add(e);

        if (ShouldPushImmediately(saved))
        {
            try
            {
                var remoteSaved = remote.Add(saved);
                if (remoteSaved.Id != saved.Id)
                {
                    local.Delete(saved);
                    saved = local.Add(remoteSaved);
                }
            }
            catch
            {
            }
        }
        return saved;
    }

    public Event Update(Event e)
    {
        e.UpdatedAt = DateTime.UtcNow;
 
        if (connectivity.IsAvailable())
        {
            var saved = remote.Update(e);
            local.Upsert(saved);
            return saved;
        }
 
        return local.Update(e);
    }
 
    public void Delete(Event e)
    {
        if (connectivity.IsAvailable())
        {
            remote.Delete(e);
            local.Delete(e);
            return;
        }
 
        // Offline: soft-delete locally so SyncService can push the deletion
        e.IsDeleted = true;
        e.UpdatedAt = DateTime.UtcNow;
        local.Update(e);
    }
 
    // -------------------------------------------------------------------------
    // Queries — always try remote when available
    // -------------------------------------------------------------------------
 
    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId,
        EventOwnerType ownerType, int ownerId)
    {
        if (connectivity.IsAvailable())
            return remote.HasOverlap(startTime, endTime, excludeId, ownerType, ownerId);
 
        return local.HasOverlap(startTime, endTime, excludeId, ownerType, ownerId);
    }
 
    public IEnumerable<Event> GetByType(EventType type, EventOwnerType ownerType, int ownerId)
    {
        if (connectivity.IsAvailable())
            return remote.GetByType(type, ownerType, ownerId);
 
        return local.GetByType(type, ownerType, ownerId);
    }
 
    public IEnumerable<Event> GetUpdatedSince(DateTime since, EventOwnerType ownerType, int ownerId)
        => local.GetUpdatedSince(since, ownerType, ownerId);
 
    public Event Upsert(Event e)
        => local.Upsert(e);

}