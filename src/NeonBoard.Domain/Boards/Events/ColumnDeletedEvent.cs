using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record ColumnDeletedEvent(
    Guid BoardId,
    Guid ColumnId,
    Guid? MovedCardsToColumnId) : IDomainEvent;
