using NeonBoard.Application.Boards.Notifications;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Common;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Notifications;

public class ColumnRealTimeNotificationHandlerTests
{
    private readonly IBoardNotificationService _notificationService = Substitute.For<IBoardNotificationService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ColumnRealTimeNotificationHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public ColumnRealTimeNotificationHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _handler = new ColumnRealTimeNotificationHandler(_notificationService, _currentUserService);
    }

    [Fact]
    public async Task Handle_ColumnAddedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var position = FractionalIndex.GenerateKeyBetween(null, null);
        var evt = new ColumnAddedEvent(boardId, Guid.NewGuid(), "To Do", position);

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "ColumnAdded", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnRenamedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new ColumnRenamedEvent(boardId, Guid.NewGuid(), "To Do", "In Progress");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "ColumnRenamed", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnDeletedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var movedToColumnId = Guid.NewGuid();
        var evt = new ColumnDeletedEvent(boardId, Guid.NewGuid(), movedToColumnId, "To Do");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "ColumnDeleted", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnDeletedEvent_WithNullMovedCardsToColumnId_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new ColumnDeletedEvent(boardId, Guid.NewGuid(), null, "Empty Column");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "ColumnDeleted", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnsReorderedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var newPositions = new Dictionary<Guid, string>
        {
            { Guid.NewGuid(), "a0" },
            { Guid.NewGuid(), "a1" }
        };
        var evt = new ColumnsReorderedEvent(boardId, newPositions);

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "ColumnsReordered", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnMovedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var newPosition = FractionalIndex.GenerateKeyBetween(null, null);
        var evt = new ColumnMovedEvent(boardId, Guid.NewGuid(), newPosition, "In Progress");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "ColumnMoved", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
