using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Boards.Commands.UpdateBoardSettings;

public class UpdateBoardSettingsHandler : IRequestHandler<UpdateBoardSettingsCommand, BoardDto>
{
    private readonly IBoardRepository _boardRepository;

    public UpdateBoardSettingsHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardDto> Handle(UpdateBoardSettingsCommand request, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (!string.Equals(board.Name, request.Name, StringComparison.Ordinal))
        {
            board.Rename(request.Name);
        }

        await _boardRepository.UpdateAsync(board, cancellationToken);

        return new BoardDto(
            board.Id,
            board.Name,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            board.Columns.Count);
    }
}
