using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Projects.Queries.GetProjectMembers;

public class GetProjectMembersHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public GetProjectMembersHandler(
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var userIds = project.Members.Select(m => m.UserId).ToList();
        var users = await _userRepository.GetByIdsAsync(userIds, cancellationToken);
        var userLookup = users.ToDictionary(u => u.Id);

        return project.Members.Select(m =>
        {
            var user = userLookup.GetValueOrDefault(m.UserId);
            return new ProjectMemberDto(
                m.UserId,
                user?.DisplayName ?? "Unknown",
                user?.Email ?? "",
                m.Role,
                m.JoinedAt);
        })
        .OrderBy(m => m.Role == ProjectRole.Owner ? 0 : m.Role == ProjectRole.Editor ? 1 : 2)
        .ThenBy(m => m.DisplayName)
        .ToList();
    }
}
