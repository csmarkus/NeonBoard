using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.Application.Boards.Queries.GetBoardsByProject;

public class GetBoardsByProjectHandler : IRequestHandler<GetBoardsByProjectQuery, List<BoardDto>>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardsByProjectHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<List<BoardDto>> Handle(GetBoardsByProjectQuery request, CancellationToken cancellationToken)
    {
        var boards = await _boardRepository.GetBoardsByProjectIdAsync(request.ProjectId, cancellationToken);

        return boards.Select(board => new BoardDto(
            board.Id,
            board.Name,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            board.Columns.Count)).ToList();
    }
}
