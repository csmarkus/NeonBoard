using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.ValueObjects;

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

        if (request.Prefix != null && !string.Equals(board.Prefix.Value, request.Prefix, StringComparison.Ordinal))
        {
            var prefixExists = await _boardRepository.PrefixExistsInProjectAsync(
                request.ProjectId, request.Prefix, request.BoardId, cancellationToken);
            if (prefixExists)
                throw new ConflictException("A board with this prefix already exists in the project.");

            board.UpdatePrefix(request.Prefix);
        }

        await _boardRepository.UpdateAsync(board, cancellationToken);

        return new BoardDto(
            board.Id,
            board.Name,
            board.Prefix.Value,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            board.Columns.Count);
    }
}
