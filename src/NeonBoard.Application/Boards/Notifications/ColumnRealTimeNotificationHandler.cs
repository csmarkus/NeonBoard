using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Notifications;

public class ColumnRealTimeNotificationHandler :
    INotificationHandler<ColumnAddedEvent>,
    INotificationHandler<ColumnRenamedEvent>,
    INotificationHandler<ColumnDeletedEvent>,
    INotificationHandler<ColumnsReorderedEvent>,
    INotificationHandler<ColumnMovedEvent>
{
    private readonly IBoardNotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public ColumnRealTimeNotificationHandler(
        IBoardNotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ColumnAddedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "ColumnAdded", new
        {
            notification.ColumnId,
            notification.Name,
            notification.Position,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(ColumnRenamedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "ColumnRenamed", new
        {
            notification.ColumnId,
            notification.OldName,
            notification.NewName,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(ColumnDeletedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "ColumnDeleted", new
        {
            notification.ColumnId,
            notification.MovedCardsToColumnId,
            notification.ColumnName,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(ColumnsReorderedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "ColumnsReordered", new
        {
            notification.NewPositions,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(ColumnMovedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "ColumnMoved", new
        {
            notification.ColumnId,
            notification.NewPosition,
            notification.ColumnName,
            ActingUserId = userId
        }, cancellationToken);
    }
}
