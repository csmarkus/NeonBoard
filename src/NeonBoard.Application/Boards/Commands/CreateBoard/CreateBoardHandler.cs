using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Boards.Commands.CreateBoard;

public class CreateBoardHandler : IRequestHandler<CreateBoardCommand, BoardDto>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateBoardHandler(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null || project.OwnerId != userId.Value)
            throw new UnauthorizedAccessException("You do not have permission to create boards in this project.");

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
