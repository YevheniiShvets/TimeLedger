using TimeLedger.Core.Interfaces.Events;

namespace BusinessCollaboration.Interfaces.Event;

public interface IRemoteEventRepository : IEventRepository, IGroupOverlapQueryable {}
