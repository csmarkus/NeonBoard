using MediatR;
using NeonBoard.Application.Columns.DTOs;

namespace NeonBoard.Application.Columns.Commands.AddColumn;

public record AddColumnCommand(Guid ProjectId, Guid BoardId, string Name) : IRequest<ColumnDto>;
