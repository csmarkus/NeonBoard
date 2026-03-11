using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Common;
using NeonBoard.UnitTests.Builders;

namespace NeonBoard.UnitTests.Domain.Boards;

public class BoardColumnTests
{
    [Fact]
    public void AddColumn_ShouldAddColumnWithCorrectPosition()
    {
        var board = new BoardBuilder().Build();

        board.AddColumn("To Do");
        board.AddColumn("In Progress");

        board.Columns.Should().HaveCount(2);
        board.Columns[0].Name.Should().Be("To Do");
        board.Columns[0].Position.Value.Should().NotBeNullOrEmpty();
        board.Columns[1].Name.Should().Be("In Progress");
        board.Columns[1].Position.Value.Should().NotBeNullOrEmpty();
        // Second column should sort after first
        string.Compare(board.Columns[0].Position.Value, board.Columns[1].Position.Value, StringComparison.Ordinal)
            .Should().BeLessThan(0);
    }

    [Fact]
    public void AddColumn_ShouldRaiseColumnAddedEvent()
    {
        var board = new BoardBuilder().Build();

        board.AddColumn("To Do");

        var domainEvent = board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ColumnAddedEvent>().Subject;

        domainEvent.BoardId.Should().Be(board.Id);
        domainEvent.Name.Should().Be("To Do");
        domainEvent.Position.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddColumn_WithEmptyName_ShouldThrowDomainException(string? name)
    {
        var board = new BoardBuilder().Build();

        var act = () => board.AddColumn(name!);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ColumnNameEmpty);
    }

    [Fact]
    public void AddColumn_WithNameExceedingMaxLength_ShouldThrowDomainException()
    {
        var board = new BoardBuilder().Build();
        var longName = new string('a', 51);

        var act = () => board.AddColumn(longName);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ColumnNameTooLong);
    }

    [Fact]
    public void RenameColumn_WithValidId_ShouldUpdateName()
    {
        var board = new BoardBuilder().WithColumn("Old Name").Build();
        var columnId = board.Columns[0].Id;

        board.RenameColumn(columnId, "New Name");

        board.Columns[0].Name.Should().Be("New Name");
    }

    [Fact]
    public void RenameColumn_ShouldRaiseColumnRenamedEvent()
    {
        var board = new BoardBuilder().WithColumn("Old Name").Build();
        var columnId = board.Columns[0].Id;

        board.RenameColumn(columnId, "New Name");

        var domainEvent = board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ColumnRenamedEvent>().Subject;

        domainEvent.BoardId.Should().Be(board.Id);
        domainEvent.ColumnId.Should().Be(columnId);
        domainEvent.OldName.Should().Be("Old Name");
        domainEvent.NewName.Should().Be("New Name");
    }

    [Fact]
    public void RenameColumn_WithInvalidId_ShouldThrowDomainException()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.RenameColumn(fakeId, "New Name");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ColumnNotFound(fakeId));
    }

    [Fact]
    public void ReorderColumns_WithValidIds_ShouldUpdatePositions()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "In Progress", "Done")
            .Build();

        var reorderedIds = new List<Guid>
        {
            board.Columns[2].Id, // Done -> first
            board.Columns[0].Id, // To Do -> second
            board.Columns[1].Id  // In Progress -> third
        };

        board.ReorderColumns(reorderedIds);

        var donePos = board.Columns.First(c => c.Name == "Done").Position.Value;
        var toDoPos = board.Columns.First(c => c.Name == "To Do").Position.Value;
        var inProgressPos = board.Columns.First(c => c.Name == "In Progress").Position.Value;

        string.Compare(donePos, toDoPos, StringComparison.Ordinal).Should().BeLessThan(0);
        string.Compare(toDoPos, inProgressPos, StringComparison.Ordinal).Should().BeLessThan(0);
    }

    [Fact]
    public void ReorderColumns_WithMismatchedCount_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .Build();

        var act = () => board.ReorderColumns([board.Columns[0].Id]);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardColumnCountMismatch);
    }

    [Fact]
    public void ReorderColumns_WithUnknownColumnId_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .Build();

        var act = () => board.ReorderColumns([board.Columns[0].Id, Guid.NewGuid()]);

        act.Should().Throw<DomainException>()
            .WithMessage("Column with ID *");
    }

    [Fact]
    public void ReorderColumns_ShouldRaiseColumnsReorderedEvent()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .Build();

        var reorderedIds = new List<Guid> { board.Columns[1].Id, board.Columns[0].Id };

        board.ReorderColumns(reorderedIds);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ColumnsReorderedEvent>();
    }

    [Fact]
    public void DeleteColumn_WithNoCards_ShouldRemoveColumn()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .Build();
        var columnId = board.Columns[0].Id;

        board.DeleteColumn(columnId);

        board.Columns.Should().HaveCount(1);
        board.Columns[0].Name.Should().Be("Done");
    }

    [Fact]
    public void DeleteColumn_ShouldNotChangeRemainingColumnPositions()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "In Progress", "Done")
            .Build();
        var toDoPos = board.Columns.First(c => c.Name == "To Do").Position.Value;
        var donePos = board.Columns.First(c => c.Name == "Done").Position.Value;
        var middleColumnId = board.Columns[1].Id;

        board.DeleteColumn(middleColumnId);

        board.Columns.Should().HaveCount(2);
        // Remaining columns' positions should be UNCHANGED (no resequencing)
        board.Columns.First(c => c.Name == "To Do").Position.Value.Should().Be(toDoPos);
        board.Columns.First(c => c.Name == "Done").Position.Value.Should().Be(donePos);
    }

    [Fact]
    public void DeleteColumn_WithCards_AndNoTarget_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .Build();
        var columnId = board.Columns[0].Id;

        var act = () => board.DeleteColumn(columnId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardCannotDeleteColumnWithCards);
    }

    [Fact]
    public void DeleteColumn_WithCards_AndTarget_ShouldMoveCards()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .Build();
        var sourceColumnId = board.Columns[0].Id;
        var targetColumnId = board.Columns[1].Id;

        board.DeleteColumn(sourceColumnId, targetColumnId);

        board.Columns.Should().HaveCount(1);
        board.Cards.Should().HaveCount(2);
        board.Cards.Should().OnlyContain(c => c.ColumnId == targetColumnId);
    }

    [Fact]
    public void DeleteColumn_ShouldRaiseColumnDeletedEvent()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;

        board.DeleteColumn(columnId);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ColumnDeletedEvent>();
    }

    [Fact]
    public void MoveColumn_ShouldUpdatePosition()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "In Progress", "Done")
            .Build();
        var columnId = board.Columns[0].Id;
        var newPosition = FractionalIndex.GenerateKeyBetween(
            board.Columns[1].Position.Value,
            board.Columns[2].Position.Value);

        board.MoveColumn(columnId, newPosition);

        board.Columns.First(c => c.Id == columnId).Position.Value.Should().Be(newPosition);
    }

    [Fact]
    public void MoveColumn_ShouldRaiseColumnMovedEvent()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .Build();
        var columnId = board.Columns[0].Id;
        var newPosition = FractionalIndex.GenerateKeyBetween(board.Columns[1].Position.Value, null);
        board.ClearDomainEvents();

        board.MoveColumn(columnId, newPosition);

        var evt = board.GetDomainEvents().OfType<ColumnMovedEvent>().Single();
        evt.ColumnId.Should().Be(columnId);
        evt.NewPosition.Should().Be(newPosition);
        evt.ColumnName.Should().Be("To Do");
    }
}
