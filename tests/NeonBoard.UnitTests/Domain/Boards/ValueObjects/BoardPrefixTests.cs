using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Boards.ValueObjects;

public class BoardPrefixTests
{
    #region Create

    [Theory]
    [InlineData("AB")]
    [InlineData("DEV")]
    [InlineData("ABCDE")]
    public void Create_WithValidPrefix_ShouldCreateBoardPrefix(string value)
    {
        var prefix = BoardPrefix.Create(value);

        prefix.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrow(string? value)
    {
        var act = () => BoardPrefix.Create(value!);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardPrefixEmpty);
    }

    [Fact]
    public void Create_WithSingleChar_ShouldThrow()
    {
        var act = () => BoardPrefix.Create("A");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardPrefixInvalid);
    }

    [Fact]
    public void Create_WithSixChars_ShouldThrow()
    {
        var act = () => BoardPrefix.Create("ABCDEF");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardPrefixInvalid);
    }

    [Theory]
    [InlineData("dev")]
    [InlineData("Dev")]
    public void Create_WithLowercase_ShouldThrow(string value)
    {
        var act = () => BoardPrefix.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardPrefixInvalid);
    }

    [Theory]
    [InlineData("D3V")]
    [InlineData("12")]
    [InlineData("AB1")]
    public void Create_WithNumbers_ShouldThrow(string value)
    {
        var act = () => BoardPrefix.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardPrefixInvalid);
    }

    [Theory]
    [InlineData("AB-")]
    [InlineData("A_B")]
    [InlineData("AB ")]
    public void Create_WithSpecialChars_ShouldThrow(string value)
    {
        var act = () => BoardPrefix.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardPrefixInvalid);
    }

    #endregion

    #region GenerateFromName

    [Fact]
    public void GenerateFromName_WithSingleLongWord_ShouldTakeFirstThreeLetters()
    {
        var prefix = BoardPrefix.GenerateFromName("Development");

        prefix.Value.Should().Be("DEV");
    }

    [Fact]
    public void GenerateFromName_WithTwoLetterWord_ShouldReturnTwoLetters()
    {
        var prefix = BoardPrefix.GenerateFromName("QA");

        prefix.Value.Should().Be("QA");
    }

    [Fact]
    public void GenerateFromName_WithTwoWords_ShouldTakeInitials()
    {
        var prefix = BoardPrefix.GenerateFromName("Sprint Board");

        prefix.Value.Should().Be("SB");
    }

    [Fact]
    public void GenerateFromName_WithFourWords_ShouldTakeInitials()
    {
        var prefix = BoardPrefix.GenerateFromName("My Cool Project Board");

        prefix.Value.Should().Be("MCPB");
    }

    [Fact]
    public void GenerateFromName_WithMoreThanFiveWords_ShouldClampToFive()
    {
        var prefix = BoardPrefix.GenerateFromName("A Really Long Board Name Here");

        prefix.Value.Should().Be("ARLBN");
    }

    [Fact]
    public void GenerateFromName_WithShortTwoLetterName_ShouldReturnTwoLetters()
    {
        var prefix = BoardPrefix.GenerateFromName("Hi");

        prefix.Value.Should().Be("HI");
    }

    [Fact]
    public void GenerateFromName_WithSingleCharName_ShouldPadToMinLength()
    {
        var prefix = BoardPrefix.GenerateFromName("X");

        prefix.Value.Should().Be("XX");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateFromName_WithNullOrEmptyOrWhitespace_ShouldThrow(string? value)
    {
        var act = () => BoardPrefix.GenerateFromName(value!);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardNameEmpty);
    }

    #endregion

    #region Equality

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var prefix1 = BoardPrefix.Create("DEV");
        var prefix2 = BoardPrefix.Create("DEV");

        prefix1.Should().Be(prefix2);
        (prefix1 == prefix2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var prefix1 = BoardPrefix.Create("DEV");
        var prefix2 = BoardPrefix.Create("QA");

        prefix1.Should().NotBe(prefix2);
        (prefix1 != prefix2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldBeSame()
    {
        var prefix1 = BoardPrefix.Create("DEV");
        var prefix2 = BoardPrefix.Create("DEV");

        prefix1.GetHashCode().Should().Be(prefix2.GetHashCode());
    }

    #endregion
}
