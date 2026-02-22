using MediatR;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Cards.Commands.ArchiveCard;

public class ArchiveCardHandler : IRequestHandler<ArchiveCardCommand, CardDto>
{
    private readonly IBoardRepository _boardRepository;

    public ArchiveCardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<CardDto> Handle(ArchiveCardCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        board.ArchiveCard(request.CardId);

        var card = board.Cards.First(c => c.Id == request.CardId);
        var labels = board.Labels.Select(l => new LabelDto(l.Id, l.Name, l.Color)).ToList();

        return new CardDto(
            card.Id,
            card.CardNumber,
            $"{board.Prefix.Value}-{card.CardNumber}",
            card.Content.Title,
            card.Content.Description,
            card.ColumnId,
            card.Position.Value,
            card.LabelIds
                .Select(labelId => labels.FirstOrDefault(l => l.Id == labelId))
                .Where(label => label != null)
                .Cast<LabelDto>()
                .ToList(),
            card.CreatedAt,
            card.UpdatedAt,
            card.ArchivedAt);
    }
}
