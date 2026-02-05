using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record CardUpdatedEvent(
    Guid BoardId,
    Guid CardId,
    string Title,
    string Description) : IDomainEvent;
