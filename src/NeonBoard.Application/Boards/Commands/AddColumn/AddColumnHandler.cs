using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Boards.Commands.AddColumn;

public class AddColumnHandler : IRequestHandler<AddColumnCommand, ColumnDto>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public AddColumnHandler(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ColumnDto> Handle(AddColumnCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null || project.OwnerId != userId.Value)
            throw new UnauthorizedAccessException("You do not have permission to modify this project.");

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
