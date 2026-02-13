using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardLabelAddedEvent(
    Guid BoardId,
    Guid CardId,
    Guid LabelId) : IDomainEvent;
