using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Activity.EventHandlers;

public class CardActivityEventHandler :
    INotificationHandler<CardCreatedEvent>,
    INotificationHandler<CardUpdatedEvent>,
    INotificationHandler<CardMovedEvent>,
    INotificationHandler<CardDeletedEvent>,
    INotificationHandler<CardArchivedEvent>,
    INotificationHandler<CardRestoredEvent>
{
    private readonly IActivityEntryRepository _repository;

    public CardActivityEventHandler(IActivityEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CardCreatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.Created,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.Title,
                ["cardNumber"] = notification.CardNumber,
                ["columnName"] = notification.ColumnName,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.Updated,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.Title,
                ["cardNumber"] = notification.CardNumber,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardMovedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.Moved,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.CardTitle,
                ["cardNumber"] = notification.CardNumber,
                ["sourceColumn"] = notification.SourceColumnName,
                ["targetColumn"] = notification.TargetColumnName,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardDeletedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.Deleted,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.CardTitle,
                ["cardNumber"] = notification.CardNumber,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardArchivedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.Archived,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.CardTitle,
                ["cardNumber"] = notification.CardNumber,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardRestoredEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.Restored,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.CardTitle,
                ["cardNumber"] = notification.CardNumber,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }
}
