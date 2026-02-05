using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record ColumnAddedEvent(
    Guid BoardId,
    Guid ColumnId,
    string Name,
    int Position) : IDomainEvent;
