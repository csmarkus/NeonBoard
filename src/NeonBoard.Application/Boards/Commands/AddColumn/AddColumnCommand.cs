using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Commands.AddColumn;

public record AddColumnCommand(Guid ProjectId, Guid BoardId, string Name) : IRequest<ColumnDto>;
