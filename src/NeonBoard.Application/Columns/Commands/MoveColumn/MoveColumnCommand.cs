using MediatR;

namespace NeonBoard.Application.Columns.Commands.MoveColumn;

public record MoveColumnCommand(Guid ProjectId, Guid BoardId, Guid ColumnId, string NewPosition) : IRequest<Unit>;
