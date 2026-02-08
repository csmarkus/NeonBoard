using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Boards.Queries.GetBoardsByProject;

public class GetBoardsByProjectHandler : IRequestHandler<GetBoardsByProjectQuery, List<BoardDto>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBoardsByProjectHandler(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<BoardDto>> Handle(GetBoardsByProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null || project.OwnerId != userId.Value)
            throw new UnauthorizedAccessException("You do not have permission to access this project.");

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
