using MediatR;

namespace NeonBoard.Application.Columns.Commands.ReorderColumns;

public record ReorderColumnsCommand(Guid ProjectId, Guid BoardId, List<Guid> ColumnIds) : IRequest<Unit>;
