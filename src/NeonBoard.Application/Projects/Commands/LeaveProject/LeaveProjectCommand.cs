using MediatR;

namespace NeonBoard.Application.Projects.Commands.LeaveProject;

public record LeaveProjectCommand(Guid ProjectId, Guid UserId) : IRequest<Unit>;
