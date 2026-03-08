using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record InvitationAcceptedEvent(
    Guid InvitationId,
    Guid ProjectId,
    Guid AcceptedByUserId,
    ProjectRole Role,
    DateTime OccurredOn) : IDomainEvent;
