using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Boards.ValueObjects;

public class PositionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Create_WithValidValue_ShouldCreatePosition(int value)
    {
        var position = Position.Create(value);

        position.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNegativeValue_ShouldThrow(int value)
    {
        var act = () => Position.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.PositionNegative);
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var position1 = Position.Create(5);
        var position2 = Position.Create(5);

        position1.Should().Be(position2);
        (position1 == position2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var position1 = Position.Create(3);
        var position2 = Position.Create(7);

        position1.Should().NotBe(position2);
        (position1 != position2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldBeSame()
    {
        var position1 = Position.Create(5);
        var position2 = Position.Create(5);

        position1.GetHashCode().Should().Be(position2.GetHashCode());
    }
}
