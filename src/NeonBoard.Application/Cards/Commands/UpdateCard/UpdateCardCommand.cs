using MediatR;

namespace NeonBoard.Application.Cards.Commands.UpdateCard;

public record UpdateCardCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid CardId,
    string Title,
    string Description) : IRequest<Unit>;
