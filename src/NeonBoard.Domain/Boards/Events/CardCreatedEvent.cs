using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardCreatedEvent(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId,
    string Title,
    string Position,
    int CardNumber,
    string ColumnName,
    string Prefix) : IDomainEvent;
