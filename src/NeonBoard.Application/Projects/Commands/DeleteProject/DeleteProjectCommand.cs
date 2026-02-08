using MediatR;

namespace NeonBoard.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid ProjectId) : IRequest<Unit>;
