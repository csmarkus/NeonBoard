using MediatR;

namespace NeonBoard.Application.Labels.Commands.RemoveLabel;

public record RemoveLabelCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid LabelId) : IRequest<Unit>;
