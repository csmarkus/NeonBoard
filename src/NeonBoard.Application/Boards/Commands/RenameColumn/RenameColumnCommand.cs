using MediatR;

namespace NeonBoard.Application.Boards.Commands.RenameColumn;

public record RenameColumnCommand(Guid ProjectId, Guid BoardId, Guid ColumnId, string NewName) : IRequest<Unit>;
