using FluentAssertions;
using NeonBoard.Domain.Boards.Entities;
using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Boards.Entities;

public class CardTests
{
    [Fact]
    public void CreateInternal_ShouldSetCardNumber()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);

        var card = Card.CreateInternal(Guid.NewGuid(), content, position, 42);

        card.CardNumber.Should().Be(42);
    }

    [Fact]
    public void CreateInternal_WithZeroCardNumber_ShouldThrowDomainException()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);

        var act = () => Card.CreateInternal(Guid.NewGuid(), content, position, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateInternal_WithNegativeCardNumber_ShouldThrowDomainException()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);

        var act = () => Card.CreateInternal(Guid.NewGuid(), content, position, -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archive_ShouldSetArchivedAt()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);
        var card = Card.CreateInternal(Guid.NewGuid(), content, position, 1);

        card.Archive();

        card.IsArchived.Should().BeTrue();
        card.ArchivedAt.Should().NotBeNull();
        card.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Restore_ShouldClearArchivedAt()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);
        var card = Card.CreateInternal(Guid.NewGuid(), content, position, 1);
        card.Archive();

        card.Restore();

        card.IsArchived.Should().BeFalse();
        card.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public void IsArchived_WhenNotArchived_ShouldBeFalse()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);
        var card = Card.CreateInternal(Guid.NewGuid(), content, position, 1);

        card.IsArchived.Should().BeFalse();
        card.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldThrowDomainException()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);
        var card = Card.CreateInternal(Guid.NewGuid(), content, position, 1);
        card.Archive();

        var act = () => card.Archive();

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardAlreadyArchived);
    }

    [Fact]
    public void Restore_WhenNotArchived_ShouldThrowDomainException()
    {
        var content = CardContent.Create("Test Card", "Description");
        var position = Position.Create(0);
        var card = Card.CreateInternal(Guid.NewGuid(), content, position, 1);

        var act = () => card.Restore();

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotArchived);
    }
}
