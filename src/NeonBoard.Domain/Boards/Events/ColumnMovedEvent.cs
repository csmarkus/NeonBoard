using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record ColumnMovedEvent(
    Guid BoardId,
    Guid ColumnId,
    string NewPosition,
    string ColumnName) : IDomainEvent;
