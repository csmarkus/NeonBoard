using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Boards;

public class BoardCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateBoard()
    {
        var projectId = Guid.NewGuid();

        var board = Board.Create("Sprint Board", projectId);

        board.Should().NotBeNull();
        board.Id.Should().NotBeEmpty();
        board.Name.Should().Be("Sprint Board");
        board.ProjectId.Should().Be(projectId);
        board.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        board.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        board.Columns.Should().BeEmpty();
        board.Cards.Should().BeEmpty();
        board.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldRaiseBoardCreatedEvent()
    {
        var projectId = Guid.NewGuid();

        var board = Board.Create("Sprint Board", projectId);

        var domainEvent = board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<BoardCreatedEvent>().Subject;

        domainEvent.BoardId.Should().Be(board.Id);
        domainEvent.Name.Should().Be("Sprint Board");
        domainEvent.ProjectId.Should().Be(projectId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowDomainException(string? name)
    {
        var act = () => Board.Create(name!, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardNameEmpty);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrowDomainException()
    {
        var longName = new string('a', 101);

        var act = () => Board.Create(longName, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardNameTooLong);
    }

    [Fact]
    public void Create_WithEmptyProjectId_ShouldThrowDomainException()
    {
        var act = () => Board.Create("Board", Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardProjectIdEmpty);
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        var board = Board.Create("Old Name", Guid.NewGuid());
        board.ClearDomainEvents();

        board.Rename("New Name");

        board.Name.Should().Be("New Name");
        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<BoardRenamedEvent>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithEmptyName_ShouldThrowDomainException(string? name)
    {
        var board = Board.Create("Board", Guid.NewGuid());

        var act = () => board.Rename(name!);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardNameEmpty);
    }

    [Fact]
    public void Create_WithExplicitPrefix_ShouldSetPrefix()
    {
        var board = Board.Create("My Board", Guid.NewGuid(), "DEV");

        board.Prefix.Value.Should().Be("DEV");
    }

    [Fact]
    public void Create_WithoutPrefix_ShouldAutoGeneratePrefix()
    {
        var board = Board.Create("Sprint Board", Guid.NewGuid());

        board.Prefix.Should().NotBeNull();
        board.Prefix.Value.Should().Be("SB");
    }

    [Fact]
    public void Create_ShouldInitializeNextCardNumberToOne()
    {
        var board = Board.Create("Test Board", Guid.NewGuid());

        board.NextCardNumber.Should().Be(1);
    }

    [Fact]
    public void Create_WithInvalidPrefix_ShouldThrowDomainException()
    {
        var act = () => Board.Create("Test Board", Guid.NewGuid(), "bad");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdatePrefix_WithValidPrefix_ShouldUpdatePrefix()
    {
        var board = Board.Create("Test Board", Guid.NewGuid(), "OLD");

        board.UpdatePrefix("NEW");

        board.Prefix.Value.Should().Be("NEW");
    }

    [Fact]
    public void UpdatePrefix_ShouldRaiseBoardPrefixUpdatedEvent()
    {
        var board = Board.Create("Test Board", Guid.NewGuid(), "OLD");
        board.ClearDomainEvents();

        board.UpdatePrefix("NEW");

        var domainEvent = board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<BoardPrefixUpdatedEvent>().Subject;
        domainEvent.BoardId.Should().Be(board.Id);
        domainEvent.OldPrefix.Should().Be("OLD");
        domainEvent.NewPrefix.Should().Be("NEW");
    }

    [Fact]
    public void UpdatePrefix_WithInvalidPrefix_ShouldThrowDomainException()
    {
        var board = Board.Create("Test Board", Guid.NewGuid(), "OLD");

        var act = () => board.UpdatePrefix("bad");

        act.Should().Throw<DomainException>();
    }
}
