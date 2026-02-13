using MediatR;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Boards.Queries.GetBoardDetails;

public class GetBoardDetailsHandler : IRequestHandler<GetBoardDetailsQuery, BoardDetailsDto>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBoardDetailsHandler(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<BoardDetailsDto> Handle(GetBoardDetailsQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null || project.OwnerId != userId.Value)
            throw new UnauthorizedAccessException("You do not have permission to access this project.");

        var board = await _boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), request.BoardId);

        if (board.ProjectId != request.ProjectId)
            throw new NotFoundException(nameof(Board), request.BoardId);

        var columns = board.Columns
            .OrderBy(c => c.Position.Value)
            .Select(c => new ColumnDto(
                c.Id,
                c.Name,
                c.Position.Value,
                board.Id))
            .ToList();

        var cards = board.Cards
            .Select(c => new CardDto(
                c.Id,
                c.Content.Title,
                c.Content.Description,
                c.ColumnId,
                c.Position.Value,
                c.GetLabelIds().ToList(),
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

        var labels = board.Labels
            .Select(l => new LabelDto(l.Id, l.Name, l.Color))
            .ToList();

        return new BoardDetailsDto(
            board.Id,
            board.Name,
            board.ProjectId,
            board.CreatedAt,
            board.UpdatedAt,
            columns,
            cards,
            labels);
    }
}
