using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record MemberRemovedFromProjectEvent(
    Guid ProjectId,
    Guid UserId,
    DateTime OccurredOn) : IDomainEvent;
