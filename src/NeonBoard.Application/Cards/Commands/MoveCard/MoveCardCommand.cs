using MediatR;

namespace NeonBoard.Application.Cards.Commands.MoveCard;

public record MoveCardCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid CardId,
    Guid TargetColumnId,
    int TargetPosition) : IRequest<Unit>;
