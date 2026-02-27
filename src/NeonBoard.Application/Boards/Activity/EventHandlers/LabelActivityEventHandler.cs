using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Activity.EventHandlers;

public class LabelActivityEventHandler :
    INotificationHandler<LabelCreatedEvent>,
    INotificationHandler<LabelUpdatedEvent>,
    INotificationHandler<LabelRemovedEvent>,
    INotificationHandler<CardLabelAddedEvent>,
    INotificationHandler<CardLabelRemovedEvent>
{
    private readonly IActivityEntryRepository _repository;

    public LabelActivityEventHandler(IActivityEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(LabelCreatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Label,
            notification.LabelId,
            ActivityActionType.Created,
            new Dictionary<string, object>
            {
                ["labelName"] = notification.Name,
                ["labelColor"] = notification.Color
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(LabelUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Label,
            notification.LabelId,
            ActivityActionType.Updated,
            new Dictionary<string, object>
            {
                ["labelName"] = notification.Name,
                ["labelColor"] = notification.Color
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(LabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Label,
            notification.LabelId,
            ActivityActionType.Deleted,
            new Dictionary<string, object>
            {
                ["labelName"] = notification.LabelName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardLabelAddedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.LabelAdded,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.CardTitle,
                ["cardNumber"] = notification.CardNumber,
                ["labelName"] = notification.LabelName,
                ["labelColor"] = notification.LabelColor,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(CardLabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Card,
            notification.CardId,
            ActivityActionType.LabelRemoved,
            new Dictionary<string, object>
            {
                ["cardTitle"] = notification.CardTitle,
                ["cardNumber"] = notification.CardNumber,
                ["labelName"] = notification.LabelName,
                ["labelColor"] = notification.LabelColor,
                ["prefix"] = notification.Prefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }
}
