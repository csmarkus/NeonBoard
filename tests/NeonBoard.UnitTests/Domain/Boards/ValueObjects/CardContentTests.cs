using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.UnitTests.Domain.Boards.ValueObjects;

public class CardContentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateContent()
    {
        var content = CardContent.Create("My Card", "A description");

        content.Title.Should().Be("My Card");
        content.Description.Should().Be("A description");
    }

    [Fact]
    public void Create_WithNullDescription_ShouldDefaultToEmpty()
    {
        var content = CardContent.Create("My Card", null);

        content.Description.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrow(string? title)
    {
        var act = () => CardContent.Create(title!, "desc");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardTitleEmpty);
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ShouldThrow()
    {
        var longTitle = new string('a', 201);

        var act = () => CardContent.Create(longTitle, "desc");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardTitleTooLong);
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldThrow()
    {
        var longDescription = new string('a', 5001);

        var act = () => CardContent.Create("Title", longDescription);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardDescriptionTooLong);
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var content1 = CardContent.Create("Title", "Desc");
        var content2 = CardContent.Create("Title", "Desc");

        content1.Should().Be(content2);
        (content1 == content2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentTitle_ShouldNotBeEqual()
    {
        var content1 = CardContent.Create("Title A", "Desc");
        var content2 = CardContent.Create("Title B", "Desc");

        content1.Should().NotBe(content2);
    }

    [Fact]
    public void Equality_DifferentDescription_ShouldNotBeEqual()
    {
        var content1 = CardContent.Create("Title", "Desc A");
        var content2 = CardContent.Create("Title", "Desc B");

        content1.Should().NotBe(content2);
    }

    [Fact]
    public void Update_ShouldReturnNewContentWithUpdatedValues()
    {
        var original = CardContent.Create("Old Title", "Old Desc");

        var updated = original.Update("New Title", "New Desc");

        updated.Title.Should().Be("New Title");
        updated.Description.Should().Be("New Desc");
        original.Title.Should().Be("Old Title");
    }
}
