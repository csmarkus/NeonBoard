using NeonBoard.Application.Boards.Activity.DTOs;
using NeonBoard.Application.Boards.Activity.Queries.GetBoardActivity;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Activity.Queries;

public class GetBoardActivityHandlerTests
{
    private readonly IActivityEntryRepository _activityRepository = Substitute.For<IActivityEntryRepository>();
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly GetBoardActivityHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetBoardActivityHandlerTests()
    {
        _handler = new GetBoardActivityHandler(_activityRepository, _boardRepository);
    }

    [Fact]
    public async Task Handle_WhenBoardExists_ShouldReturnActivityFeed()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var entries = new List<ActivityEntry>
        {
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, Guid.NewGuid(), ActivityActionType.Created, new Dictionary<string, object> { ["cardTitle"] = "Card 1" }),
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Column, Guid.NewGuid(), ActivityActionType.Created, new Dictionary<string, object> { ["columnName"] = "To Do" })
        };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetBoardActivityAsync(boardId, 21, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBoardActivityQuery(projectId, boardId, 20, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(2);
        result.Entries[0].BoardId.Should().Be(boardId);
        result.Entries[0].UserId.Should().Be(_userId);
        result.Entries[0].UserName.Should().Be("Alice");
        result.Entries[0].EntityType.Should().Be("Card");
        result.Entries[0].ActionType.Should().Be("Created");
        result.Entries[1].EntityType.Should().Be("Column");
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenMoreEntriesThanPageSize_ShouldReturnNextCursor()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var pageSize = 2;

        // Create 3 entries (pageSize + 1) to simulate "has more"
        var entries = new List<ActivityEntry>
        {
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, Guid.NewGuid(), ActivityActionType.Created, new Dictionary<string, object> { ["cardTitle"] = "Card 1" }),
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, Guid.NewGuid(), ActivityActionType.Updated, new Dictionary<string, object> { ["cardTitle"] = "Card 2" }),
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Column, Guid.NewGuid(), ActivityActionType.Created, new Dictionary<string, object> { ["columnName"] = "Col" })
        };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetBoardActivityAsync(boardId, pageSize + 1, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBoardActivityQuery(projectId, boardId, pageSize, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Entries.Should().HaveCount(pageSize);
        result.NextCursor.Should().NotBeNull();
        result.NextCursor.Should().Be(entries[1].OccurredAt);
    }

    [Fact]
    public async Task Handle_WhenFewerEntriesThanPageSize_ShouldReturnNullNextCursor()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var pageSize = 20;

        var entries = new List<ActivityEntry>
        {
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, Guid.NewGuid(), ActivityActionType.Created, new Dictionary<string, object> { ["cardTitle"] = "Card 1" })
        };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetBoardActivityAsync(boardId, pageSize + 1, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBoardActivityQuery(projectId, boardId, pageSize, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Entries.Should().HaveCount(1);
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenBoardDoesNotExist_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new GetBoardActivityQuery(projectId, boardId, 20, null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithCursor_ShouldPassCursorToRepository()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cursor = DateTime.UtcNow.AddMinutes(-5);

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetBoardActivityAsync(boardId, 21, cursor, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityEntry>());

        var query = new GetBoardActivityQuery(projectId, boardId, 20, cursor);

        var result = await _handler.Handle(query, CancellationToken.None);

        await _activityRepository.Received(1).GetBoardActivityAsync(boardId, 21, cursor, Arg.Any<CancellationToken>());
        result.Entries.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapEntriesToDtosCorrectly()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var data = new Dictionary<string, object> { ["cardTitle"] = "Test Card", ["columnName"] = "To Do" };

        var entry = ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, entityId, ActivityActionType.Created, data);
        var entries = new List<ActivityEntry> { entry };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetBoardActivityAsync(boardId, 21, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetBoardActivityQuery(projectId, boardId, 20, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Entries.Single();
        dto.Id.Should().Be(entry.Id);
        dto.BoardId.Should().Be(boardId);
        dto.UserId.Should().Be(_userId);
        dto.UserName.Should().Be("Alice");
        dto.EntityType.Should().Be("Card");
        dto.EntityId.Should().Be(entityId);
        dto.ActionType.Should().Be("Created");
        dto.Data.Should().ContainKey("cardTitle");
        dto.Data["cardTitle"].Should().Be("Test Card");
        dto.OccurredAt.Should().Be(entry.OccurredAt);
    }
}
