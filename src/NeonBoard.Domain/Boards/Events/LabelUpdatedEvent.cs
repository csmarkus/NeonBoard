using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record LabelUpdatedEvent(
    Guid BoardId,
    Guid LabelId,
    string Name,
    string Color) : IDomainEvent;
