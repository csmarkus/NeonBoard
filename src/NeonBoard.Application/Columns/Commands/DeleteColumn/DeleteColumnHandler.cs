using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Common;

namespace NeonBoard.Application.Columns.Commands.DeleteColumn;

public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public DeleteColumnHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
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

        return Unit.Value;
    }
}
