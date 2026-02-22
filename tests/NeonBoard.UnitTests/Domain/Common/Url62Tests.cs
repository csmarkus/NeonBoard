using FluentAssertions;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Common;

public class Url62Tests
{
    [Fact]
    public void Generate_ShouldReturnStringOfRequestedLength()
    {
        var result = Url62.Generate(7);
        result.Should().HaveLength(7);
    }

    [Fact]
    public void Generate_WithUnambiguousFlag_ShouldExcludeAmbiguousCharacters()
    {
        var ambiguousChars = new[] { '0', '1', 'O', 'I', 'l' };

        for (int i = 0; i < 100; i++)
        {
            var result = Url62.Generate(20, unambiguous: true);
            result.Should().NotContainAny(ambiguousChars.Select(c => c.ToString()).ToArray());
        }
    }

    [Fact]
    public void Generate_ShouldReturnDifferentValues()
    {
        var results = Enumerable.Range(0, 10).Select(_ => Url62.Generate(7)).ToHashSet();
        results.Count.Should().BeGreaterThan(1);
    }
}
