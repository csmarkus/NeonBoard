using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Events;

public record InvitationCreatedEvent(
    Guid InvitationId,
    Guid ProjectId,
    string Email,
    ProjectRole Role,
    Guid InvitedByUserId,
    DateTime OccurredOn) : IDomainEvent;
