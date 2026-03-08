using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Infrastructure.Services;

public class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectAuthorizationService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<bool> HasRoleAsync(Guid projectId, Guid userId, ProjectRole requiredRole, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetWithMembersAsync(projectId, cancellationToken);
        if (project == null)
            return false;

        var memberRole = project.GetMemberRole(userId);
        if (memberRole == null)
            return false;

        return memberRole.Value >= requiredRole;
    }

    public async Task EnsureRoleAsync(Guid projectId, Guid userId, ProjectRole requiredRole, CancellationToken cancellationToken = default)
    {
        if (!await HasRoleAsync(projectId, userId, requiredRole, cancellationToken))
        {
            throw new UnauthorizedAccessException(
                $"User does not have the required role '{requiredRole}' for this project.");
        }
    }
}
