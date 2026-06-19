using TimeLedger.Core.Models.Event;

namespace BusinessCollaboration.Interfaces.Event;

public interface IGroupOverlapQueryable
{
    IEnumerable<int> GetOverlappingOwnerIds(DateTime startTime, DateTime endTime, int? excludeId, EventOwnerType ownerType, IEnumerable<int> ownerIds);
}