using MediatR;

namespace NeonBoard.Application.Boards.Commands.DeleteBoard;

public record DeleteBoardCommand(Guid ProjectId, Guid BoardId) : IRequest<Unit>;
