using FluentAssertions;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Common;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Sprint Board", "sprint-board")]
    [InlineData("My Project (v2)", "my-project-v2")]
    [InlineData("  Hello  World  ", "hello-world")]
    [InlineData("UPPERCASE", "uppercase")]
    [InlineData("special!@#chars", "special-chars")]
    [InlineData("multiple---hyphens", "multiple-hyphens")]
    [InlineData("-leading-trailing-", "leading-trailing")]
    public void Slugify_ShouldConvertToUrlFriendlyString(string input, string expected)
    {
        var result = SlugHelper.Slugify(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void Slugify_EmptyString_ShouldThrow()
    {
        var act = () => SlugHelper.Slugify("");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Slugify_NullString_ShouldThrow()
    {
        var act = () => SlugHelper.Slugify(null!);
        act.Should().Throw<DomainException>();
    }
}
