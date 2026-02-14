using MediatR;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Columns.Commands.AddColumn;

public class AddColumnHandler : IRequestHandler<AddColumnCommand, ColumnDto>
{
    private readonly IBoardRepository _boardRepository;

    public AddColumnHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<ColumnDto> Handle(AddColumnCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var columnId = board.AddColumn(request.Name);

        var column = board.Columns.First(c => c.Id == columnId);
        return new ColumnDto(
            column.Id,
            column.Name,
            column.Position.Value,
            board.Id);
    }
}
