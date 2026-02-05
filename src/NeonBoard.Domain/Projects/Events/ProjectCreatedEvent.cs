using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record ProjectCreatedEvent(
    Guid ProjectId,
    string Name,
    Guid OwnerId,
    DateTime CreatedAt) : IDomainEvent;
