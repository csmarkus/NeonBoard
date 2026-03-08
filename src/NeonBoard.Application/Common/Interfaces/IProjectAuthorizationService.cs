using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Common.Interfaces;

public interface IProjectAuthorizationService
{
    Task<bool> HasRoleAsync(Guid projectId, Guid userId, ProjectRole requiredRole, CancellationToken cancellationToken = default);
    Task EnsureRoleAsync(Guid projectId, Guid userId, ProjectRole requiredRole, CancellationToken cancellationToken = default);
}
