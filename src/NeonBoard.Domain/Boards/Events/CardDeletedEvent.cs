using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardDeletedEvent(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId,
    string CardTitle,
    int CardNumber,
    string Prefix) : IDomainEvent;
