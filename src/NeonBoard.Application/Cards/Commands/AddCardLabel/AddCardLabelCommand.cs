using MediatR;

namespace NeonBoard.Application.Cards.Commands.AddCardLabel;

public record AddCardLabelCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid CardId,
    Guid LabelId) : IRequest<Unit>;
