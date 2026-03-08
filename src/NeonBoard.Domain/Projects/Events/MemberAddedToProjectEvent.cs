using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record MemberAddedToProjectEvent(
    Guid ProjectId,
    Guid UserId,
    ProjectRole Role,
    DateTime OccurredOn) : IDomainEvent;
