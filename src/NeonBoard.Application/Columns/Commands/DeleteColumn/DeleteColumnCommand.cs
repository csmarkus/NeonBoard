using MediatR;

namespace NeonBoard.Application.Columns.Commands.DeleteColumn;

public record DeleteColumnCommand(Guid ProjectId, Guid BoardId, Guid ColumnId) : IRequest<Unit>;
