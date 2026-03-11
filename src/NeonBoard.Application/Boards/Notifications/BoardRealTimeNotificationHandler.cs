using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Notifications;

public class BoardRealTimeNotificationHandler :
    INotificationHandler<BoardCreatedEvent>,
    INotificationHandler<BoardRenamedEvent>,
    INotificationHandler<BoardDeletedEvent>
{
    private readonly IBoardNotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBoardRepository _boardRepository;

    public BoardRealTimeNotificationHandler(
        IBoardNotificationService notificationService,
        ICurrentUserService currentUserService,
        IBoardRepository boardRepository)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _boardRepository = boardRepository;
    }

    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendProjectEventAsync(notification.ProjectId, "BoardCreated", new
        {
            notification.BoardId,
            notification.Name,
            notification.ProjectId,
            notification.CreatedAt,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(BoardRenamedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        var board = await _boardRepository.GetByIdAsync(notification.BoardId, cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "BoardRenamed", new
        {
            notification.BoardId,
            notification.OldName,
            notification.NewName,
            ActingUserId = userId
        }, cancellationToken);

        if (board != null)
        {
            await _notificationService.SendProjectEventAsync(board.ProjectId, "BoardRenamed", new
            {
                notification.BoardId,
                notification.OldName,
                notification.NewName,
                ActingUserId = userId
            }, cancellationToken);
        }
    }

    public async Task Handle(BoardDeletedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "BoardDeleted", new
        {
            notification.BoardId,
            notification.ProjectId,
            notification.BoardName,
            ActingUserId = userId
        }, cancellationToken);

        await _notificationService.SendProjectEventAsync(notification.ProjectId, "BoardDeleted", new
        {
            notification.BoardId,
            notification.ProjectId,
            notification.BoardName,
            ActingUserId = userId
        }, cancellationToken);
    }
}
