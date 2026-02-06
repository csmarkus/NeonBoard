using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Commands.CreateProject;

public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;

    public CreateProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(
            request.Name,
            request.Description,
            request.OwnerId);

        await _projectRepository.AddAsync(project, cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.CreatedAt,
            project.UpdatedAt);
    }
}
