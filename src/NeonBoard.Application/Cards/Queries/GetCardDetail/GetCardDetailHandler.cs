using MediatR;
using NeonBoard.Application.Boards.Activity.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Entities;

namespace NeonBoard.Application.Cards.Queries.GetCardDetail;

public class GetCardDetailHandler : IRequestHandler<GetCardDetailQuery, CardDetailDto>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IActivityEntryRepository _activityRepository;

    public GetCardDetailHandler(
        IBoardRepository boardRepository,
        IActivityEntryRepository activityRepository)
    {
        _boardRepository = boardRepository;
        _activityRepository = activityRepository;
    }

    public async Task<CardDetailDto> Handle(GetCardDetailQuery request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);

        if (board is null || board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var card = board.Cards.FirstOrDefault(c => c.Id == request.CardId);
        if (card is null)
            throw new NotFoundException(nameof(Card), request.CardId);

        var boardLabels = board.Labels
            .Select(l => new LabelDto(l.Id, l.Name, l.Color))
            .ToList();

        var cardDto = CardDto.FromCard(card, board.Prefix.Value, boardLabels);

        const int pageSize = 10;
        var entries = await _activityRepository.GetCardActivityAsync(
            request.BoardId,
            request.CardId,
            pageSize + 1,
            null,
            cancellationToken);

        var hasMore = entries.Count > pageSize;
        var pageEntries = hasMore ? entries.Take(pageSize).ToList() : entries;

        var activityDtos = pageEntries.Select(e => new ActivityEntryDto(
            e.Id,
            e.BoardId,
            e.UserId,
            e.UserName,
            e.EntityType.ToString(),
            e.EntityId,
            e.ActionType.ToString(),
            e.Data,
            e.OccurredAt)).ToList();

        var nextCursor = hasMore ? pageEntries.Last().OccurredAt : (DateTime?)null;
        var activityFeed = new ActivityFeedDto(activityDtos, nextCursor);

        return new CardDetailDto(
            cardDto.Id,
            cardDto.CardNumber,
            cardDto.DisplayId,
            cardDto.Title,
            cardDto.Description,
            cardDto.ColumnId,
            cardDto.Position,
            cardDto.Labels,
            cardDto.CreatedAt,
            cardDto.UpdatedAt,
            cardDto.ArchivedAt,
            activityFeed);
    }
}
