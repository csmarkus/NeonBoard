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
    private readonly ICurrentUserService _currentUserService;

    public BoardActivityEventHandler(
        IActivityEntryRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
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
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
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
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
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
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
            ActivityEntityType.Board,
            notification.BoardId,
            ActivityActionType.Deleted,
            new Dictionary<string, object>
            {
                ["boardName"] = notification.BoardName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    private async Task<(Guid UserId, string UserName)?> GetUserContextAsync(CancellationToken cancellationToken)
    {
        var userId = await _currentUserService.GetUserIdAsync(cancellationToken);
        if (userId == null)
            return null;

        return (userId.Value, _currentUserService.Name ?? "Unknown");
    }
}
