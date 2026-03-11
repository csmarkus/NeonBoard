using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Boards.ValueObjects;

public class PositionTests
{
    [Fact]
    public void Create_WithValidStringValue_ShouldCreatePosition()
    {
        var position = Position.Create("a0");

        position.Value.Should().Be("a0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrow(string? value)
    {
        var act = () => Position.Create(value!);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.PositionEmpty);
    }

    [Fact]
    public void Initial_ShouldReturnValidPosition()
    {
        var position = Position.Initial();

        position.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Between_WithBeforeAndAfter_ShouldReturnPositionBetween()
    {
        var before = Position.Create("a0");
        var after = Position.Create("a2");

        var between = Position.Between(before, after);

        string.Compare(before.Value, between.Value, StringComparison.Ordinal).Should().BeNegative();
        string.Compare(between.Value, after.Value, StringComparison.Ordinal).Should().BeNegative();
    }

    [Fact]
    public void Between_WithNullBefore_ShouldReturnPositionBeforeAfter()
    {
        var after = Position.Create("a0");

        var result = Position.Between(null, after);

        string.Compare(result.Value, after.Value, StringComparison.Ordinal).Should().BeNegative();
    }

    [Fact]
    public void Between_WithNullAfter_ShouldReturnPositionAfterBefore()
    {
        var before = Position.Create("a0");

        var result = Position.Between(before, null);

        string.Compare(before.Value, result.Value, StringComparison.Ordinal).Should().BeNegative();
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var position1 = Position.Create("a0");
        var position2 = Position.Create("a0");

        position1.Should().Be(position2);
        (position1 == position2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var position1 = Position.Create("a0");
        var position2 = Position.Create("a1");

        position1.Should().NotBe(position2);
        (position1 != position2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldBeSame()
    {
        var position1 = Position.Create("a0");
        var position2 = Position.Create("a0");

        position1.GetHashCode().Should().Be(position2.GetHashCode());
    }
}
