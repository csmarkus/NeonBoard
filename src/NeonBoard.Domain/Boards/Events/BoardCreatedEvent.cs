using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Events;

public record BoardCreatedEvent(
    Guid BoardId,
    string Name,
    Guid ProjectId,
    DateTime CreatedAt) : IDomainEvent;
