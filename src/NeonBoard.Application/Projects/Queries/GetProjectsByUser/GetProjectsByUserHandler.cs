using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;

namespace NeonBoard.Application.Projects.Queries.GetProjectsByUser;

public class GetProjectsByUserHandler : IRequestHandler<GetProjectsByUserQuery, List<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsByUserHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsByUserQuery request, CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetProjectsByUserIdAsync(request.UserId, cancellationToken);

        return projects.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.Description,
            p.OwnerId,
            p.CreatedAt,
            p.UpdatedAt))
            .ToList();
    }
}
