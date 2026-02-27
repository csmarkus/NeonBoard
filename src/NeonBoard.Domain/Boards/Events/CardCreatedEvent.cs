using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardCreatedEvent(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId,
    string Title,
    int Position,
    int CardNumber,
    string ColumnName,
    string Prefix) : IDomainEvent;
