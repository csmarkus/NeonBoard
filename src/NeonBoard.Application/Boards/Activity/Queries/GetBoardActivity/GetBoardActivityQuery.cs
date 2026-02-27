using MediatR;
using NeonBoard.Application.Boards.Activity.DTOs;

namespace NeonBoard.Application.Boards.Activity.Queries.GetBoardActivity;

public record GetBoardActivityQuery(
    Guid ProjectId,
    Guid BoardId,
    int PageSize,
    DateTime? Cursor) : IRequest<ActivityFeedDto>;
