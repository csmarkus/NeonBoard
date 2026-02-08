using MediatR;

namespace NeonBoard.Application.Boards.Commands.DeleteColumn;

public record DeleteColumnCommand(Guid ProjectId, Guid BoardId, Guid ColumnId) : IRequest<Unit>;
