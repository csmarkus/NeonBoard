using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Boards.Commands.DeleteColumn;

public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteColumnHandler(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null || project.OwnerId != userId.Value)
            throw new UnauthorizedAccessException("You do not have permission to modify this project.");

        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var cardsInColumn = board.Cards.Count(c => c.ColumnId == request.ColumnId);
        if (cardsInColumn > 0)
        {
            throw new DomainException($"Cannot delete column. It contains {cardsInColumn} card(s). Please move or delete the cards first.");
        }

        board.DeleteColumn(request.ColumnId, null);

        await _boardRepository.UpdateAsync(board, cancellationToken);

        return Unit.Value;
    }
}
