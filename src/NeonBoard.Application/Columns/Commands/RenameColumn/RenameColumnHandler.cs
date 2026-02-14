using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Columns.Commands.RenameColumn;

public class RenameColumnHandler : IRequestHandler<RenameColumnCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public RenameColumnHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(RenameColumnCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.RenameColumn(request.ColumnId, request.NewName);

        return Unit.Value;
    }
}
