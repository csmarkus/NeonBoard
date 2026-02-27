using MediatR;
using NeonBoard.Application.Boards.Activity.DTOs;

namespace NeonBoard.Application.Boards.Activity.Queries.GetCardActivity;

public record GetCardActivityQuery(
    Guid ProjectId,
    Guid BoardId,
    Guid CardId,
    int PageSize,
    DateTime? Cursor) : IRequest<ActivityFeedDto>;
