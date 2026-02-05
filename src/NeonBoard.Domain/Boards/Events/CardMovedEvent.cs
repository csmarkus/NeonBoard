using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardMovedEvent(
    Guid BoardId,
    Guid CardId,
    Guid SourceColumnId,
    Guid TargetColumnId,
    int TargetPosition) : IDomainEvent;
