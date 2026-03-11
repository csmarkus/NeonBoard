using NeonBoard.Application.Boards.Notifications;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Common;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Notifications;

public class CardRealTimeNotificationHandlerTests
{
    private readonly IBoardNotificationService _notificationService = Substitute.For<IBoardNotificationService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly CardRealTimeNotificationHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CardRealTimeNotificationHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _handler = new CardRealTimeNotificationHandler(_notificationService, _currentUserService);
    }

    [Fact]
    public async Task Handle_CardCreatedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var position = FractionalIndex.GenerateKeyBetween(null, null);
        var evt = new CardCreatedEvent(boardId, Guid.NewGuid(), Guid.NewGuid(), "Test Card", position, 1, "Todo", "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardCreated", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardUpdatedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardUpdatedEvent(boardId, Guid.NewGuid(), "Updated Title", "Updated Description", 5, "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardUpdated", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardMovedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var newPosition = FractionalIndex.GenerateKeyBetween(null, null);
        var evt = new CardMovedEvent(
            boardId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            newPosition, "Test Card", 1, "Todo", "Done", "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardMoved", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardDeletedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardDeletedEvent(boardId, Guid.NewGuid(), Guid.NewGuid(), "Test Card", 1, "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardDeleted", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardArchivedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardArchivedEvent(boardId, Guid.NewGuid(), Guid.NewGuid(), "Archived Card", 3, "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardArchived", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardRestoredEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardRestoredEvent(boardId, Guid.NewGuid(), Guid.NewGuid(), "Restored Card", 4, "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardRestored", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardHeldEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardHeldEvent(boardId, Guid.NewGuid(), Guid.NewGuid(), "Held Card", 5, "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardHeld", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardResumedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardResumedEvent(boardId, Guid.NewGuid(), Guid.NewGuid(), "Resumed Card", 6, "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardResumed", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardLabelAddedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardLabelAddedEvent(
            boardId, Guid.NewGuid(), Guid.NewGuid(), "Test Card", 7, "Bug", "#ff0000", "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardLabelAdded", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardLabelRemovedEvent_ShouldSendBoardEvent()
    {
        var boardId = Guid.NewGuid();
        var evt = new CardLabelRemovedEvent(
            boardId, Guid.NewGuid(), Guid.NewGuid(), "Test Card", 8, "Bug", "#ff0000", "TST");

        await _handler.Handle(evt, CancellationToken.None);

        await _notificationService.Received(1)
            .SendBoardEventAsync(boardId, "CardLabelRemoved", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
