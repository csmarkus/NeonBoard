using Microsoft.EntityFrameworkCore;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Infrastructure.Persistence;

namespace NeonBoard.Infrastructure.Repositories;

public class ActivityEntryRepository : IActivityEntryRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityEntryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ActivityEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.ActivityEntries.AddAsync(entry, cancellationToken);
    }

    public async Task<List<ActivityEntry>> GetBoardActivityAsync(
        Guid boardId,
        int pageSize,
        DateTime? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityEntries
            .AsNoTracking()
            .Where(e => e.BoardId == boardId);

        if (cursor.HasValue)
        {
            query = query.Where(e => e.OccurredAt < cursor.Value);
        }

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ActivityEntry>> GetCardActivityAsync(
        Guid boardId,
        Guid cardId,
        int pageSize,
        DateTime? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityEntries
            .AsNoTracking()
            .Where(e => e.BoardId == boardId && e.EntityId == cardId);

        if (cursor.HasValue)
        {
            query = query.Where(e => e.OccurredAt < cursor.Value);
        }

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
