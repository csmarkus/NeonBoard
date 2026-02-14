using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Cards.Commands.RemoveCardLabel;

public class RemoveCardLabelHandler : IRequestHandler<RemoveCardLabelCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;

    public RemoveCardLabelHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Unit> Handle(RemoveCardLabelCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.RemoveLabelFromCard(request.CardId, request.LabelId);

        return Unit.Value;
    }
}
