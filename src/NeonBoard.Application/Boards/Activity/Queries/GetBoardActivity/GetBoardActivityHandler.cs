using MediatR;
using NeonBoard.Application.Boards.Activity.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Boards.Activity.Queries.GetBoardActivity;

public class GetBoardActivityHandler : IRequestHandler<GetBoardActivityQuery, ActivityFeedDto>
{
    private readonly IActivityEntryRepository _activityRepository;
    private readonly IBoardRepository _boardRepository;

    public GetBoardActivityHandler(
        IActivityEntryRepository activityRepository,
        IBoardRepository boardRepository)
    {
        _activityRepository = activityRepository;
        _boardRepository = boardRepository;
    }

    public async Task<ActivityFeedDto> Handle(GetBoardActivityQuery request, CancellationToken cancellationToken)
    {
        var boardExists = await _boardRepository.BoardExistsInProjectAsync(
            request.BoardId, request.ProjectId, cancellationToken);

        if (!boardExists)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Fetch one extra to determine if there are more pages
        var entries = await _activityRepository.GetBoardActivityAsync(
            request.BoardId,
            pageSize + 1,
            request.Cursor,
            cancellationToken);

        var hasMore = entries.Count > pageSize;
        var pageEntries = hasMore ? entries.Take(pageSize).ToList() : entries;

        var dtos = pageEntries.Select(e => new ActivityEntryDto(
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

        return new ActivityFeedDto(dtos, nextCursor);
    }
}
