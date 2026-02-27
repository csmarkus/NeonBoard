using NeonBoard.Domain.Boards.Activity;

namespace NeonBoard.UnitTests.Domain.Boards.Activity;

public class ActivityEntryTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var boardId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var data = new Dictionary<string, object> { ["cardTitle"] = "Fix bug" };

        var entry = ActivityEntry.Create(
            boardId,
            ActivityEntityType.Card,
            entityId,
            ActivityActionType.Created,
            data);

        entry.Id.Should().NotBeEmpty();
        entry.BoardId.Should().Be(boardId);
        entry.EntityType.Should().Be(ActivityEntityType.Card);
        entry.EntityId.Should().Be(entityId);
        entry.ActionType.Should().Be(ActivityActionType.Created);
        entry.Data.Should().ContainKey("cardTitle");
        entry.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entry1 = ActivityEntry.Create(Guid.NewGuid(), ActivityEntityType.Board, Guid.NewGuid(), ActivityActionType.Created, new());
        var entry2 = ActivityEntry.Create(Guid.NewGuid(), ActivityEntityType.Board, Guid.NewGuid(), ActivityActionType.Created, new());

        entry1.Id.Should().NotBe(entry2.Id);
    }
}
