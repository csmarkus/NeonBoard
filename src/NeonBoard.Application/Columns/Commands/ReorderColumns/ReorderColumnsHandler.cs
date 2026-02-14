using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Columns.Commands.ReorderColumns;

public class ReorderColumnsHandler : IRequestHandler<ReorderColumnsCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public ReorderColumnsHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(ReorderColumnsCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.ReorderColumns(request.ColumnIds);

        return Unit.Value;
    }
}
