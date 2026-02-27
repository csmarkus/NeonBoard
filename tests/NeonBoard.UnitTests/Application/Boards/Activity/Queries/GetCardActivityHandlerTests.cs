using NeonBoard.Application.Boards.Activity.DTOs;
using NeonBoard.Application.Boards.Activity.Queries.GetCardActivity;
using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Boards.Activity;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Boards.Activity.Queries;

public class GetCardActivityHandlerTests
{
    private readonly IActivityEntryRepository _activityRepository = Substitute.For<IActivityEntryRepository>();
    private readonly IBoardRepository _boardRepository = Substitute.For<IBoardRepository>();
    private readonly GetCardActivityHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetCardActivityHandlerTests()
    {
        _handler = new GetCardActivityHandler(_activityRepository, _boardRepository);
    }

    [Fact]
    public async Task Handle_WhenBoardExists_ShouldReturnCardActivityFeed()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();

        var entries = new List<ActivityEntry>
        {
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, cardId, ActivityActionType.Created, new Dictionary<string, object> { ["cardTitle"] = "Card 1" }),
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, cardId, ActivityActionType.Updated, new Dictionary<string, object> { ["cardTitle"] = "Card 1" })
        };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetCardActivityAsync(boardId, cardId, 21, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetCardActivityQuery(projectId, boardId, cardId, 20, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Entries.Should().HaveCount(2);
        result.Entries[0].EntityId.Should().Be(cardId);
        result.Entries[0].UserId.Should().Be(_userId);
        result.Entries[0].UserName.Should().Be("Alice");
        result.Entries[0].ActionType.Should().Be("Created");
        result.Entries[1].ActionType.Should().Be("Updated");
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenBoardDoesNotExist_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new GetCardActivityQuery(projectId, boardId, cardId, 20, null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenMoreEntriesThanPageSize_ShouldReturnNextCursor()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var pageSize = 2;

        var entries = new List<ActivityEntry>
        {
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, cardId, ActivityActionType.Created, new Dictionary<string, object> { ["cardTitle"] = "Card 1" }),
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, cardId, ActivityActionType.Updated, new Dictionary<string, object> { ["cardTitle"] = "Card 1" }),
            ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, cardId, ActivityActionType.Moved, new Dictionary<string, object> { ["cardTitle"] = "Card 1" })
        };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetCardActivityAsync(boardId, cardId, pageSize + 1, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetCardActivityQuery(projectId, boardId, cardId, pageSize, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Entries.Should().HaveCount(pageSize);
        result.NextCursor.Should().NotBeNull();
        result.NextCursor.Should().Be(entries[1].OccurredAt);
    }

    [Fact]
    public async Task Handle_WithCursor_ShouldPassCursorToRepository()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var cursor = DateTime.UtcNow.AddMinutes(-10);

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetCardActivityAsync(boardId, cardId, 21, cursor, Arg.Any<CancellationToken>())
            .Returns(new List<ActivityEntry>());

        var query = new GetCardActivityQuery(projectId, boardId, cardId, 20, cursor);

        var result = await _handler.Handle(query, CancellationToken.None);

        await _activityRepository.Received(1).GetCardActivityAsync(boardId, cardId, 21, cursor, Arg.Any<CancellationToken>());
        result.Entries.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapEntriesToDtosCorrectly()
    {
        var projectId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var data = new Dictionary<string, object> { ["cardTitle"] = "My Card", ["fromColumn"] = "To Do", ["toColumn"] = "Done" };

        var entry = ActivityEntry.Create(boardId, _userId, "Alice", ActivityEntityType.Card, cardId, ActivityActionType.Moved, data);
        var entries = new List<ActivityEntry> { entry };

        _boardRepository.BoardExistsInProjectAsync(boardId, projectId, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.GetCardActivityAsync(boardId, cardId, 21, null, Arg.Any<CancellationToken>())
            .Returns(entries);

        var query = new GetCardActivityQuery(projectId, boardId, cardId, 20, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Entries.Single();
        dto.Id.Should().Be(entry.Id);
        dto.BoardId.Should().Be(boardId);
        dto.UserId.Should().Be(_userId);
        dto.UserName.Should().Be("Alice");
        dto.EntityType.Should().Be("Card");
        dto.EntityId.Should().Be(cardId);
        dto.ActionType.Should().Be("Moved");
        dto.Data.Should().ContainKey("cardTitle");
        dto.Data.Should().ContainKey("fromColumn");
        dto.Data.Should().ContainKey("toColumn");
        dto.OccurredAt.Should().Be(entry.OccurredAt);
    }
}
