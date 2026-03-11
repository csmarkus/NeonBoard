using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Columns.Commands.MoveColumn;

public class MoveColumnHandler : IRequestHandler<MoveColumnCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public MoveColumnHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(MoveColumnCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.MoveColumn(request.ColumnId, request.NewPosition);

        return Unit.Value;
    }
}
