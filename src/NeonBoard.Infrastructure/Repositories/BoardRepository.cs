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
        // Owned entities (Columns and Cards) are automatically loaded, no need for Include
        return await DbSet
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
}
