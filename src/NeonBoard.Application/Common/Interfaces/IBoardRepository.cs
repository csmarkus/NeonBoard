using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Common.Interfaces;

public interface IBoardRepository : IRepository<Board>
{
    Task<Board?> GetBoardWithDetailsAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<List<Board>> GetBoardsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> BoardExistsInProjectAsync(Guid boardId, Guid projectId, CancellationToken cancellationToken = default);
}
