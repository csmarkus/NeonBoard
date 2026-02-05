using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record ColumnsReorderedEvent(
    Guid BoardId,
    Dictionary<Guid, int> NewPositions) : IDomainEvent;
