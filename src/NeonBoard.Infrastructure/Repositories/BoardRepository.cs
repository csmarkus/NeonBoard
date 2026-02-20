using Microsoft.EntityFrameworkCore;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Infrastructure.Persistence;

namespace NeonBoard.Infrastructure.Repositories;

public class BoardRepository : Repository<Board>, IBoardRepository
{
    public BoardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Board?> GetBoardWithDetailsAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsTracking()
            .Include(b => b.Columns)
            .Include(b => b.Cards)
            .Include(b => b.Labels)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
    }

    public async Task<List<Board>> GetBoardsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.ProjectId == projectId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> BoardExistsInProjectAsync(Guid boardId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(b => b.Id == boardId && b.ProjectId == projectId, cancellationToken);
    }

    public async Task<bool> PrefixExistsInProjectAsync(Guid projectId, string prefix, Guid? excludeBoardId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(b => b.ProjectId == projectId && b.Prefix.Value == prefix);

        if (excludeBoardId.HasValue)
            query = query.Where(b => b.Id != excludeBoardId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
