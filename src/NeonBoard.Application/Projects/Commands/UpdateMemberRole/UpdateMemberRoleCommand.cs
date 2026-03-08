using MediatR;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand(Guid ProjectId, Guid UserId, ProjectRole NewRole) : IRequest<Unit>;
