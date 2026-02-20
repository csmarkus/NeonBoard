using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record BoardPrefixUpdatedEvent(Guid BoardId, string OldPrefix, string NewPrefix) : IDomainEvent;
