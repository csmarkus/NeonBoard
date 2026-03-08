using MediatR;

namespace NeonBoard.Application.Projects.Commands.AcceptInvitation;

public record AcceptInvitationCommand(string Token, Guid UserId) : IRequest;
