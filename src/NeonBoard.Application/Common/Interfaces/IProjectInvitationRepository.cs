using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Common.Interfaces;

public interface IProjectInvitationRepository : IRepository<ProjectInvitation>
{
    Task<ProjectInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<ProjectInvitation>> GetPendingByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingInvitationAsync(Guid projectId, string email, CancellationToken cancellationToken = default);
}
