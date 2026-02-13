using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record LabelRemovedEvent(
    Guid BoardId,
    Guid LabelId) : IDomainEvent;
