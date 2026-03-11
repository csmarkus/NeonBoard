using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardMovedEvent(
    Guid BoardId,
    Guid CardId,
    Guid SourceColumnId,
    Guid TargetColumnId,
    string NewPosition,
    string CardTitle,
    int CardNumber,
    string SourceColumnName,
    string TargetColumnName,
    string Prefix) : IDomainEvent;
