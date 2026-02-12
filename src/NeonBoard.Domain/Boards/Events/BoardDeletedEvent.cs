using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record BoardDeletedEvent(Guid BoardId, Guid ProjectId) : IDomainEvent;
