using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;

namespace NeonBoard.Application.Boards.Commands.CreateBoard;

public class CreateBoardHandler : IRequestHandler<CreateBoardCommand, BoardDto>
{
    private readonly IBoardRepository _boardRepository;

    public CreateBoardHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = Board.Create(request.Name, request.ProjectId);

        await _boardRepository.AddAsync(board, cancellationToken);

        return new BoardDto(
            board.Id,
            board.Name,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            board.Columns.Count);
    }
}
