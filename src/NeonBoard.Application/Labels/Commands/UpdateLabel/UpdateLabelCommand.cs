using MediatR;

namespace NeonBoard.Application.Labels.Commands.UpdateLabel;

public record UpdateLabelCommand(
    Guid ProjectId,
    Guid BoardId,
    Guid LabelId,
    string Name,
    string Color) : IRequest<Unit>;
