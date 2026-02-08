using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Queries.GetProject;

public class GetProjectHandler : IRequestHandler<GetProjectQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectHandler(
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null || project.OwnerId != userId.Value)
            throw new UnauthorizedAccessException("You do not have permission to access this project.");

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.CreatedAt,
            project.UpdatedAt);
    }
}
