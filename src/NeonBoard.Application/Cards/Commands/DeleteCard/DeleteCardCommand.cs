using MediatR;

namespace NeonBoard.Application.Cards.Commands.DeleteCard;

public record DeleteCardCommand(Guid ProjectId, Guid BoardId, Guid CardId) : IRequest<Unit>;
