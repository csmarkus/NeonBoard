using MediatR;
using NeonBoard.Application.Labels.DTOs;

namespace NeonBoard.Application.Labels.Commands.AddLabel;

public record AddLabelCommand(
    Guid ProjectId,
    Guid BoardId,
    string Name,
    string Color) : IRequest<LabelDto>;
