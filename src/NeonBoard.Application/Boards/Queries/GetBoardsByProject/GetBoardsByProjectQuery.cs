using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Queries.GetBoardsByProject;

public record GetBoardsByProjectQuery(Guid ProjectId) : IRequest<List<BoardDto>>;
