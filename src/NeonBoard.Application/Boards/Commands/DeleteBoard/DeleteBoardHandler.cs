using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Boards.Commands.DeleteBoard;

public class DeleteBoardHandler : IRequestHandler<DeleteBoardCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public DeleteBoardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        await _boardRepository.DeleteAsync(board, cancellationToken);

        return Unit.Value;
    }
}
