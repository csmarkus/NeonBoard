using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Common.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<List<Project>> GetProjectsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsProjectOwnedByUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<Project?> GetWithMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<List<Project>> GetProjectsByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
