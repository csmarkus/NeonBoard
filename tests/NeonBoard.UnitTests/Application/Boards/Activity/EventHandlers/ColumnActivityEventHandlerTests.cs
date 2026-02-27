using NeonBoard.Application.Boards.Activity.EventHandlers;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NeonBoard.Domain.Boards.Events;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Activity.EventHandlers;

public class ColumnActivityEventHandlerTests
{
    private readonly IActivityEntryRepository _repository = Substitute.For<IActivityEntryRepository>();
    private readonly ColumnActivityEventHandler _handler;

    public ColumnActivityEventHandlerTests()
    {
        _handler = new ColumnActivityEventHandler(_repository);
    }

    [Fact]
    public async Task Handle_ColumnAddedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var evt = new ColumnAddedEvent(boardId, columnId, "To Do", 0);

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.EntityType == ActivityEntityType.Column &&
                e.EntityId == columnId &&
                e.ActionType == ActivityActionType.Created &&
                (string)e.Data["columnName"] == "To Do"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnDeletedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var evt = new ColumnDeletedEvent(boardId, columnId, null, "Done");

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.EntityType == ActivityEntityType.Column &&
                e.EntityId == columnId &&
                e.ActionType == ActivityActionType.Deleted &&
                (string)e.Data["columnName"] == "Done"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ColumnsReorderedEvent_ShouldPersistActivityEntry()
    {
        var boardId = Guid.NewGuid();
        var positions = new Dictionary<Guid, int>
        {
            [Guid.NewGuid()] = 0,
            [Guid.NewGuid()] = 1
        };
        var evt = new ColumnsReorderedEvent(boardId, positions);

        await _handler.Handle(evt, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ActivityEntry>(e =>
                e.BoardId == boardId &&
                e.EntityType == ActivityEntityType.Column &&
                e.EntityId == boardId &&
                e.ActionType == ActivityActionType.Reordered &&
                e.Data.Count == 0),
            Arg.Any<CancellationToken>());
    }
}
