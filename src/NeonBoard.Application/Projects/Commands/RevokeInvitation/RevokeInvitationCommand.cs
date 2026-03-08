using MediatR;

namespace NeonBoard.Application.Projects.Commands.RevokeInvitation;

public record RevokeInvitationCommand(Guid ProjectId, Guid InvitationId) : IRequest<Unit>;
