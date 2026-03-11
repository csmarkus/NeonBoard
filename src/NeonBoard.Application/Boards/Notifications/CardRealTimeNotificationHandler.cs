using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Notifications;

public class CardRealTimeNotificationHandler :
    INotificationHandler<CardCreatedEvent>,
    INotificationHandler<CardUpdatedEvent>,
    INotificationHandler<CardMovedEvent>,
    INotificationHandler<CardDeletedEvent>,
    INotificationHandler<CardArchivedEvent>,
    INotificationHandler<CardRestoredEvent>,
    INotificationHandler<CardHeldEvent>,
    INotificationHandler<CardResumedEvent>,
    INotificationHandler<CardLabelAddedEvent>,
    INotificationHandler<CardLabelRemovedEvent>
{
    private readonly IBoardNotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public CardRealTimeNotificationHandler(
        IBoardNotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CardCreatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardCreated", new
        {
            notification.CardId,
            notification.ColumnId,
            notification.Title,
            notification.Position,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardUpdated", new
        {
            notification.CardId,
            notification.Title,
            notification.Description,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardMovedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardMoved", new
        {
            notification.CardId,
            notification.SourceColumnId,
            notification.TargetColumnId,
            notification.NewPosition,
            notification.CardTitle,
            notification.CardNumber,
            notification.SourceColumnName,
            notification.TargetColumnName,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardDeletedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardDeleted", new
        {
            notification.CardId,
            notification.ColumnId,
            notification.CardTitle,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardArchivedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardArchived", new
        {
            notification.CardId,
            notification.ColumnId,
            notification.CardTitle,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardRestoredEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardRestored", new
        {
            notification.CardId,
            notification.ColumnId,
            notification.CardTitle,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardHeldEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardHeld", new
        {
            notification.CardId,
            notification.ColumnId,
            notification.CardTitle,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardResumedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardResumed", new
        {
            notification.CardId,
            notification.ColumnId,
            notification.CardTitle,
            notification.CardNumber,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardLabelAddedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardLabelAdded", new
        {
            notification.CardId,
            notification.LabelId,
            notification.CardTitle,
            notification.CardNumber,
            notification.LabelName,
            notification.LabelColor,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }

    public async Task Handle(CardLabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);

        await _notificationService.SendBoardEventAsync(notification.BoardId, "CardLabelRemoved", new
        {
            notification.CardId,
            notification.LabelId,
            notification.CardTitle,
            notification.CardNumber,
            notification.LabelName,
            notification.LabelColor,
            notification.Prefix,
            ActingUserId = userId
        }, cancellationToken);
    }
}
