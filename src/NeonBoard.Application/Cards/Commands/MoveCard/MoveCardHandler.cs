using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Cards.Commands.MoveCard;

public class MoveCardHandler : IRequestHandler<MoveCardCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public MoveCardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(MoveCardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.MoveCard(request.CardId, request.TargetColumnId, request.TargetPosition);

        return Unit.Value;
    }
}
