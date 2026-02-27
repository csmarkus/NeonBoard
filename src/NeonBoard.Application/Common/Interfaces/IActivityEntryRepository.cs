using NeonBoard.Domain.Boards.Activity;

namespace NeonBoard.Application.Common.Interfaces;

public interface IActivityEntryRepository
{
    Task AddAsync(ActivityEntry entry, CancellationToken cancellationToken = default);

    Task<List<ActivityEntry>> GetBoardActivityAsync(
        Guid boardId,
        int pageSize,
        DateTime? cursor = null,
        CancellationToken cancellationToken = default);

    Task<List<ActivityEntry>> GetCardActivityAsync(
        Guid boardId,
        Guid cardId,
        int pageSize,
        DateTime? cursor = null,
        CancellationToken cancellationToken = default);
}
