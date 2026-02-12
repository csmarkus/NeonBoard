using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Commands.RenameBoard;

public record RenameBoardCommand(Guid ProjectId, Guid BoardId, string Name) : IRequest<BoardDto>;
