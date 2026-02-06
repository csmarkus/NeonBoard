using MediatR;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    string Name,
    string Description,
    Guid OwnerId) : IRequest<ProjectDto>;
