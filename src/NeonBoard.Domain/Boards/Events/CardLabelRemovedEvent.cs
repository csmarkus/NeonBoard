using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardLabelRemovedEvent(
    Guid BoardId,
    Guid CardId,
    Guid LabelId) : IDomainEvent;
