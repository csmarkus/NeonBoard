using NeonBoard.Application.Boards.Activity.EventHandlers;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Activity.EventHandlers;

public class CardActivityEventHandlerTests
{
    private readonly IActivityEntryRepository _repository = Substitute.For<IActivityEntryRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly CardActivityEventHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CardActivityEventHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _currentUserService.Name.Returns("Test User");
        _handler = new CardActivityEventHandler(_repository, _currentUserService);
    }

    [Fact]
    public async Task Handle_CardCreatedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var evt = new CardCreatedEvent(boardId, cardId, columnId, "Fix login bug", 0, 42, "To Do", "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.Created &&
                (string)e.Data["cardTitle"] == "Fix login bug" &&
                (int)e.Data["cardNumber"] == 42 &&
                (string)e.Data["columnName"] == "To Do" &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardUpdatedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var evt = new CardUpdatedEvent(boardId, cardId, "Updated title", "New description", 5, "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.Updated &&
                (string)e.Data["cardTitle"] == "Updated title" &&
                (int)e.Data["cardNumber"] == 5 &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardMovedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var sourceColumnId = Guid.NewGuid();
        var targetColumnId = Guid.NewGuid();
        var evt = new CardMovedEvent(
            boardId, cardId, sourceColumnId, targetColumnId, 0,
            "Fix login bug", 3, "To Do", "In Progress", "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.Moved &&
                (string)e.Data["cardTitle"] == "Fix login bug" &&
                (int)e.Data["cardNumber"] == 3 &&
                (string)e.Data["sourceColumn"] == "To Do" &&
                (string)e.Data["targetColumn"] == "In Progress" &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardDeletedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var evt = new CardDeletedEvent(boardId, cardId, columnId, "Old card", 7, "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.Deleted &&
                (string)e.Data["cardTitle"] == "Old card" &&
                (int)e.Data["cardNumber"] == 7 &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardArchivedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var evt = new CardArchivedEvent(boardId, cardId, columnId, "Archived card", 12, "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.Archived &&
                (string)e.Data["cardTitle"] == "Archived card" &&
                (int)e.Data["cardNumber"] == 12 &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardRestoredEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var evt = new CardRestoredEvent(boardId, cardId, columnId, "Restored card", 15, "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.Restored &&
                (string)e.Data["cardTitle"] == "Restored card" &&
                (int)e.Data["cardNumber"] == 15 &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }
}
