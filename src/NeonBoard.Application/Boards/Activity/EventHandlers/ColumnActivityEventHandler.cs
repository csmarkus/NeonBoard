using MediatR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;

namespace NeonBoard.Application.Boards.Activity.EventHandlers;

public class ColumnActivityEventHandler :
    INotificationHandler<ColumnAddedEvent>,
    INotificationHandler<ColumnDeletedEvent>,
    INotificationHandler<ColumnRenamedEvent>,
    INotificationHandler<ColumnsReorderedEvent>
{
    private readonly IActivityEntryRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ColumnActivityEventHandler(
        IActivityEntryRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ColumnAddedEvent notification, CancellationToken cancellationToken)
    {
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
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
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
            ActivityEntityType.Column,
            notification.ColumnId,
            ActivityActionType.Deleted,
            new Dictionary<string, object>
            {
                ["columnName"] = notification.ColumnName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(ColumnRenamedEvent notification, CancellationToken cancellationToken)
    {
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
            ActivityEntityType.Column,
            notification.ColumnId,
            ActivityActionType.Updated,
            new Dictionary<string, object>
            {
                ["oldName"] = notification.OldName,
                ["newName"] = notification.NewName
            });

        await _repository.AddAsync(entry, cancellationToken);
    }

    public async Task Handle(ColumnsReorderedEvent notification, CancellationToken cancellationToken)
    {
        var userContext = await GetUserContextAsync(cancellationToken);
        if (userContext == null)
            return;

        var (userId, userName) = userContext.Value;

        var entry = ActivityEntry.Create(
            notification.BoardId,
            userId,
            userName,
            ActivityEntityType.Column,
            notification.BoardId,
            ActivityActionType.Reordered,
            new Dictionary<string, object>());

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
