using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardArchivedEvent(
    Guid BoardId,
    Guid CardId,
    Guid ColumnId) : IDomainEvent;
