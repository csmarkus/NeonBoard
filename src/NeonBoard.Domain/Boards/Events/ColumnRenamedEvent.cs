using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record ColumnRenamedEvent(
    Guid BoardId,
    Guid ColumnId,
    string OldName,
    string NewName) : IDomainEvent;
