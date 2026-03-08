using MediatR;

namespace NeonBoard.Application.Projects.Commands.RemoveMember;

public record RemoveMemberCommand(Guid ProjectId, Guid UserId) : IRequest;
