using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Commands.CreateBoard;

public record CreateBoardCommand(Guid ProjectId, string Name) : IRequest<BoardDto>;
