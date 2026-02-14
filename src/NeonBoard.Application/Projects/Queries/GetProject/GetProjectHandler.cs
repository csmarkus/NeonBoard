using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Queries.GetProject;

public class GetProjectHandler : IRequestHandler<GetProjectQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.CreatedAt,
            project.UpdatedAt);
    }
}
