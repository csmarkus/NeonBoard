using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Queries.GetBoardDetails;

public record GetBoardDetailsQuery(Guid ProjectId, Guid BoardId) : IRequest<BoardDetailsDto>;
