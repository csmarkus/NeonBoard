using Microsoft.EntityFrameworkCore;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;
using NeonBoard.Infrastructure.Persistence;

namespace NeonBoard.Infrastructure.Repositories;

public class ProjectInvitationRepository : Repository<ProjectInvitation>, IProjectInvitationRepository
{
    public ProjectInvitationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ProjectInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<List<ProjectInvitation>> GetPendingByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.ProjectId == projectId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPendingInvitationAsync(Guid projectId, string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(i => i.ProjectId == projectId
                && i.Email == email
                && i.Status == InvitationStatus.Pending,
                cancellationToken);
    }
}
