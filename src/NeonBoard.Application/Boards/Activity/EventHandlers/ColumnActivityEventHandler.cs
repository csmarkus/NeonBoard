using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Activity.EventHandlers;

public class ColumnActivityEventHandler :
    INotificationHandler<ColumnAddedEvent>,
    INotificationHandler<ColumnDeletedEvent>,
    INotificationHandler<ColumnsReorderedEvent>
{
    private readonly IActivityEntryRepository _repository;

    public ColumnActivityEventHandler(IActivityEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ColumnAddedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Column,
            notification.ColumnId,
            ActivityActionType.Created,
            new Dictionary<string, object>
            {
                ["columnName"] = notification.Name
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(ColumnDeletedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Column,
            notification.ColumnId,
            ActivityActionType.Deleted,
            new Dictionary<string, object>
            {
                ["columnName"] = notification.ColumnName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(ColumnsReorderedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Column,
            notification.BoardId,
            ActivityActionType.Reordered,
            new Dictionary<string, object>());

        await _repository.AddAsync(entry, cancellationToken);
    }
}
