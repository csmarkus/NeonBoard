using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Notifications;

public class LabelRealTimeNotificationHandler :
    INotificationHandler<LabelCreatedEvent>,
    INotificationHandler<LabelUpdatedEvent>,
    INotificationHandler<LabelRemovedEvent>
{
    private readonly IBoardNotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public LabelRealTimeNotificationHandler(
        IBoardNotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(LabelCreatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "LabelCreated", new
        {
            notification.LabelId,
            notification.Name,
            notification.Color,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(LabelUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "LabelUpdated", new
        {
            notification.LabelId,
            notification.Name,
            notification.Color,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(LabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "LabelRemoved", new
        {
            notification.LabelId,
            notification.LabelName,
            ActingUserId = userId
        }, cancellationToken);
    }
}
