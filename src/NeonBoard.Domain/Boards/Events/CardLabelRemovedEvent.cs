using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardLabelRemovedEvent(
    Guid BoardId,
    Guid CardId,
    Guid LabelId,
    string CardTitle,
    int CardNumber,
    string LabelName,
    string LabelColor,
    string Prefix) : IDomainEvent;
