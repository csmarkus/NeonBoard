using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record ProjectUpdatedEvent(
    Guid ProjectId,
    string Name,
    string Description) : IDomainEvent;
