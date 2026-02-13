using MediatR;

namespace NeonBoard.Application.Cards.Commands.RemoveCardLabel;

public record RemoveCardLabelCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid CardId,
    Guid LabelId) : IRequest<Unit>;
