using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardRestoredEvent(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId) : IDomainEvent;
