using NeonBoard.Application.Boards.Activity.EventHandlers;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Activity.EventHandlers;

public class BoardActivityEventHandlerTests
{
    private readonly IActivityEntryRepository _repository = Substitute.For<IActivityEntryRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly BoardActivityEventHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public BoardActivityEventHandlerTests()
    {
        _currentUserService.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _currentUserService.Name.Returns("Test User");
        _handler = new BoardActivityEventHandler(_repository, _currentUserService);
    }

    [Fact]
    public async Task Handle_BoardCreatedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var evt = new BoardCreatedEvent(boardId, "Sprint Board", projectId, DateTime.UtcNow);

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Board &&
                e.EntityId == boardId &&
                e.ActionType == ActivityActionType.Created &&
                (string)e.Data["boardName"] == "Sprint Board"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardRenamedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var evt = new BoardRenamedEvent(boardId, "Old Name", "New Name");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Board &&
                e.EntityId == boardId &&
                e.ActionType == ActivityActionType.Renamed &&
                (string)e.Data["oldName"] == "Old Name" &&
                (string)e.Data["newName"] == "New Name"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardPrefixUpdatedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var evt = new BoardPrefixUpdatedEvent(boardId, "OLD", "NEW");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Board &&
                e.EntityId == boardId &&
                e.ActionType == ActivityActionType.PrefixUpdated &&
                (string)e.Data["oldPrefix"] == "OLD" &&
                (string)e.Data["newPrefix"] == "NEW"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BoardDeletedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var evt = new BoardDeletedEvent(boardId, projectId, "Sprint Board");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.UserId == _userId &&
                e.UserName == "Test User" &&
                e.EntityType == ActivityEntityType.Board &&
                e.EntityId == boardId &&
                e.ActionType == ActivityActionType.Deleted &&
                (string)e.Data["boardName"] == "Sprint Board"),
            Arg.Any<CancellationToken>());
    }
}
