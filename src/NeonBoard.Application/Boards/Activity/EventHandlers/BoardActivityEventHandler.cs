using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Activity.EventHandlers;

public class BoardActivityEventHandler :
    INotificationHandler<BoardCreatedEvent>,
    INotificationHandler<BoardRenamedEvent>,
    INotificationHandler<BoardPrefixUpdatedEvent>,
    INotificationHandler<BoardDeletedEvent>
{
    private readonly IActivityEntryRepository _repository;

    public BoardActivityEventHandler(IActivityEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Board,
            notification.BoardId,
            ActivityActionType.Created,
            new Dictionary<string, object>
            {
                ["boardName"] = notification.Name
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(BoardRenamedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Board,
            notification.BoardId,
            ActivityActionType.Renamed,
            new Dictionary<string, object>
            {
                ["oldName"] = notification.OldName,
                ["newName"] = notification.NewName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(BoardPrefixUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Board,
            notification.BoardId,
            ActivityActionType.PrefixUpdated,
            new Dictionary<string, object>
            {
                ["oldPrefix"] = notification.OldPrefix,
                ["newPrefix"] = notification.NewPrefix
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(BoardDeletedEvent notification, CancellationToken cancellationToken)
    {
        var entry = ActivityEntry.Create(
            notification.BoardId,
            ActivityEntityType.Board,
            notification.BoardId,
            ActivityActionType.Deleted,
            new Dictionary<string, object>
            {
                ["boardName"] = notification.BoardName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }
}
