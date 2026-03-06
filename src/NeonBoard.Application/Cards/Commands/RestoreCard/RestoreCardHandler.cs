using MediatR;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Cards.Commands.RestoreCard;

public class RestoreCardHandler : IRequestHandler<RestoreCardCommand, CardDto>
{
    private readonly IBoardRepository _boardRepository;

    public RestoreCardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<CardDto> Handle(RestoreCardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.RestoreCard(request.CardId);

        var card = board.Cards.First(c => c.Id == request.CardId);
        var labels = board.Labels.Select(l => new LabelDto(l.Id, l.Name, l.Color)).ToList();

        return CardDto.FromCard(card, board.Prefix.Value, labels);
    }
}
