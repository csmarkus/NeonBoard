using MediatR;

namespace NeonBoard.Application.Boards.Commands.DeleteCard;

public record DeleteCardCommand(Guid ProjectId, Guid BoardId, Guid CardId) : IRequest<Unit>;
