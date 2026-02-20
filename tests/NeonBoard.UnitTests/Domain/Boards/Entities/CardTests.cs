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
}
