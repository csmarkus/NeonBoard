using MediatR;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Cards.Commands.AddCard;

public class AddCardHandler : IRequestHandler<AddCardCommand, CardDto>
{
    private readonly IBoardRepository _boardRepository;

    public AddCardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<CardDto> Handle(AddCardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var cardId = board.AddCard(request.ColumnId, request.Title, request.Description ?? string.Empty);

        var card = board.Cards.First(c => c.Id == cardId);
        return new CardDto(
            card.Id,
            card.Content.Title,
            card.Content.Description,
            card.ColumnId,
            card.Position.Value,
            [],
            card.CreatedAt,
            card.UpdatedAt);
    }
}
