using MediatR;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Application.Cards.Commands.UpdateCard;

public class UpdateCardHandler : IRequestHandler<UpdateCardCommand, Unit>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCardHandler(
        IBoardRepository boardRepository,
        IProjectRepository projectRepository,
        ICurrentUserService currentUserService)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
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

        board.UpdateCard(request.CardId, request.Title, request.Description ?? string.Empty);

        return Unit.Value;
    }
}
