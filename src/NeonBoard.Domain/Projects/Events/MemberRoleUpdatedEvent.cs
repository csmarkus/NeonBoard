using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record MemberRoleUpdatedEvent(
    Guid ProjectId,
    Guid UserId,
    ProjectRole OldRole,
    ProjectRole NewRole,
    DateTime OccurredOn) : IDomainEvent;
