using NeonBoard.Application.Boards.Activity.EventHandlers;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Activity.EventHandlers;

public class LabelActivityEventHandlerTests
{
    private readonly IActivityEntryRepository _repository = Substitute.For<IActivityEntryRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly LabelActivityEventHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public LabelActivityEventHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _currentUserService.Name.Returns("Test User");
        _handler = new LabelActivityEventHandler(_repository, _currentUserService);
    }

    [Fact]
    public async Task Handle_LabelCreatedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var evt = new LabelCreatedEvent(boardId, labelId, "Bug", "#ff0000");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Label &&
                e.EntityId == labelId &&
                e.ActionType == ActivityActionType.Created &&
                (string)e.Data["labelName"] == "Bug" &&
                (string)e.Data["labelColor"] == "#ff0000"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LabelUpdatedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var evt = new LabelUpdatedEvent(boardId, labelId, "Feature", "#00ff00");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Label &&
                e.EntityId == labelId &&
                e.ActionType == ActivityActionType.Updated &&
                (string)e.Data["labelName"] == "Feature" &&
                (string)e.Data["labelColor"] == "#00ff00"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LabelRemovedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var evt = new LabelRemovedEvent(boardId, labelId, "Deprecated");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Label &&
                e.EntityId == labelId &&
                e.ActionType == ActivityActionType.Deleted &&
                (string)e.Data["labelName"] == "Deprecated"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardLabelAddedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var evt = new CardLabelAddedEvent(boardId, cardId, labelId, "Fix login bug", 3, "Bug", "#ff0000", "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.LabelAdded &&
                (string)e.Data["cardTitle"] == "Fix login bug" &&
                (int)e.Data["cardNumber"] == 3 &&
                (string)e.Data["labelName"] == "Bug" &&
                (string)e.Data["labelColor"] == "#ff0000" &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CardLabelRemovedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var evt = new CardLabelRemovedEvent(boardId, cardId, labelId, "Fix login bug", 3, "Bug", "#ff0000", "SPR");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Card &&
                e.EntityId == cardId &&
                e.ActionType == ActivityActionType.LabelRemoved &&
                (string)e.Data["cardTitle"] == "Fix login bug" &&
                (int)e.Data["cardNumber"] == 3 &&
                (string)e.Data["labelName"] == "Bug" &&
                (string)e.Data["labelColor"] == "#ff0000" &&
                (string)e.Data["prefix"] == "SPR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldNotPersistActivityEntry()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns((Guid?)null);
        var boardId = Guid.NewGuid();
        var labelId = Guid.NewGuid();
        var evt = new LabelCreatedEvent(boardId, labelId, "Bug", "#ff0000");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.DidNotReceive().AddAsync(Arg.Any<ActivityEntry>(), Arg.Any<CancellationToken>());
    }
}
