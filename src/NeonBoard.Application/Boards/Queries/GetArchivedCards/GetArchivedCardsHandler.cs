using MediatR;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Boards.Queries.GetArchivedCards;

public class GetArchivedCardsHandler : IRequestHandler<GetArchivedCardsQuery, List<CardDto>>
{
    private readonly IBoardRepository _boardRepository;

    public GetArchivedCardsHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<List<CardDto>> Handle(GetArchivedCardsQuery request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var labels = board.Labels
            .OrderBy(l => l.Name)
            .Select(l => new LabelDto(l.Id, l.Name, l.Color))
            .ToList();

        var cards = board.Cards
            .Where(c => c.IsArchived)
            .OrderByDescending(c => c.ArchivedAt)
            .Select(c => new CardDto(
                c.Id,
                c.CardNumber,
                $"{board.Prefix.Value}-{c.CardNumber}",
                c.Content.Title,
                c.Content.Description,
                c.ColumnId,
                c.Position.Value,
                c.LabelIds
                    .Select(labelId => labels.FirstOrDefault(l => l.Id == labelId))
                    .Where(label => label != null)
                    .Cast<LabelDto>()
                    .OrderBy(label => label.Name)
                    .ToList(),
                c.CreatedAt,
                c.UpdatedAt,
                c.ArchivedAt))
            .ToList();

        return cards;
    }
}
