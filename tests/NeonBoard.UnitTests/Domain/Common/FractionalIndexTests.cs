using FluentAssertions;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Common;

public class FractionalIndexTests
{
    [Fact]
    public void GenerateKeyBetween_BothNull_ReturnsDefaultKey()
    {
        var result = FractionalIndex.GenerateKeyBetween(null, null);

        result.Should().Be("a0");
    }

    [Fact]
    public void GenerateKeyBetween_BeforeNull_ReturnsKeyLessThanAfter()
    {
        var result = FractionalIndex.GenerateKeyBetween(null, "a1");

        result.Should().NotBeNullOrEmpty();
        string.Compare(result, "a1", StringComparison.Ordinal).Should().BeLessThan(0);
    }

    [Fact]
    public void GenerateKeyBetween_AfterNull_ReturnsKeyGreaterThanBefore()
    {
        var result = FractionalIndex.GenerateKeyBetween("a1", null);

        result.Should().NotBeNullOrEmpty();
        string.Compare(result, "a1", StringComparison.Ordinal).Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateKeyBetween_BetweenTwoKeys_ReturnsKeyInBetween()
    {
        var result = FractionalIndex.GenerateKeyBetween("a0", "a2");

        result.Should().NotBeNullOrEmpty();
        string.Compare(result, "a0", StringComparison.Ordinal).Should().BeGreaterThan(0);
        string.Compare(result, "a2", StringComparison.Ordinal).Should().BeLessThan(0);
    }

    [Fact]
    public void GenerateKeyBetween_AdjacentKeys_ReturnsKeyBetweenThem()
    {
        var result = FractionalIndex.GenerateKeyBetween("a0", "a1");

        result.Should().NotBeNullOrEmpty();
        string.Compare(result, "a0", StringComparison.Ordinal).Should().BeGreaterThan(0);
        string.Compare(result, "a1", StringComparison.Ordinal).Should().BeLessThan(0);
    }

    [Fact]
    public void GenerateKeyBetween_SameKeys_ThrowsArgumentException()
    {
        var act = () => FractionalIndex.GenerateKeyBetween("a1", "a1");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateKeyBetween_BeforeGreaterThanAfter_ThrowsArgumentException()
    {
        var act = () => FractionalIndex.GenerateKeyBetween("a2", "a1");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateNKeysBetween_BothNull_ReturnsFiveAscendingKeys()
    {
        var result = FractionalIndex.GenerateNKeysBetween(null, null, 5);

        result.Should().HaveCount(5);
        for (int i = 0; i < result.Count - 1; i++)
        {
            string.Compare(result[i], result[i + 1], StringComparison.Ordinal)
                .Should().BeLessThan(0, "keys must be in ascending order");
        }
    }

    [Fact]
    public void GenerateNKeysBetween_WithBounds_ReturnsKeysInRange()
    {
        var result = FractionalIndex.GenerateNKeysBetween("a0", "a5", 3);

        result.Should().HaveCount(3);

        string.Compare("a0", result[0], StringComparison.Ordinal)
            .Should().BeLessThan(0, "first key must be greater than lower bound");

        string.Compare(result[^1], "a5", StringComparison.Ordinal)
            .Should().BeLessThan(0, "last key must be less than upper bound");

        for (int i = 0; i < result.Count - 1; i++)
        {
            string.Compare(result[i], result[i + 1], StringComparison.Ordinal)
                .Should().BeLessThan(0, "keys must be in ascending order");
        }
    }

    [Fact]
    public void GenerateNKeysBetween_ZeroCount_ReturnsEmptyList()
    {
        var result = FractionalIndex.GenerateNKeysBetween(null, null, 0);

        result.Should().BeEmpty();
    }

    [Fact]
    public void StressTest_SequentialAppends_ProduceAscendingOrder()
    {
        var keys = new List<string>();
        string? lastKey = null;

        for (int i = 0; i < 50; i++)
        {
            var newKey = FractionalIndex.GenerateKeyBetween(lastKey, null);
            keys.Add(newKey);
            lastKey = newKey;
        }

        for (int i = 0; i < keys.Count - 1; i++)
        {
            string.Compare(keys[i], keys[i + 1], StringComparison.Ordinal)
                .Should().BeLessThan(0,
                    $"key[{i}]='{keys[i]}' should be less than key[{i + 1}]='{keys[i + 1]}'");
        }
    }

    [Fact]
    public void StressTest_MiddleInsertions_ProduceValidOrdering()
    {
        var lower = "a0";
        var upper = "a1";

        var keys = new List<string> { lower, upper };

        for (int i = 0; i < 20; i++)
        {
            var newKey = FractionalIndex.GenerateKeyBetween(lower, upper);
            keys.Add(newKey);

            string.Compare(newKey, lower, StringComparison.Ordinal)
                .Should().BeGreaterThan(0,
                    $"insertion {i}: '{newKey}' should be greater than lower '{lower}'");
            string.Compare(newKey, upper, StringComparison.Ordinal)
                .Should().BeLessThan(0,
                    $"insertion {i}: '{newKey}' should be less than upper '{upper}'");

            // Narrow the window: insert between lower and the new key next time
            upper = newKey;
        }

        keys.Sort(StringComparer.Ordinal);
        for (int i = 0; i < keys.Count - 1; i++)
        {
            string.Compare(keys[i], keys[i + 1], StringComparison.Ordinal)
                .Should().BeLessThan(0,
                    $"sorted key[{i}]='{keys[i]}' should be less than key[{i + 1}]='{keys[i + 1]}'");
        }
    }
}
