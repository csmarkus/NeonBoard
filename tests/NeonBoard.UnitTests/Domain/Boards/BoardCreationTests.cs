using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Events;
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
}
