using NeonBoard.Domain.Boards;
using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Common;
using NeonBoard.UnitTests.Builders;

namespace NeonBoard.UnitTests.Domain.Boards;

public class BoardLabelTests
{
    [Fact]
    public void AddLabel_ShouldAddLabelAndRaiseEvent()
    {
        var board = new BoardBuilder().Build();

        var labelId = board.AddLabel("Bug", LabelColors.Red);

        board.Labels.Should().HaveCount(1);
        board.Labels[0].Id.Should().Be(labelId);
        board.Labels[0].Name.Should().Be("Bug");
        board.Labels[0].Color.Should().Be(LabelColors.Red);

        var domainEvent = board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<LabelCreatedEvent>().Subject;

        domainEvent.LabelId.Should().Be(labelId);
        domainEvent.Name.Should().Be("Bug");
        domainEvent.Color.Should().Be(LabelColors.Red);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddLabel_WithEmptyName_ShouldThrow(string? name)
    {
        var board = new BoardBuilder().Build();

        var act = () => board.AddLabel(name!, LabelColors.Blue);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.LabelNameEmpty);
    }

    [Fact]
    public void AddLabel_WithNameExceedingMaxLength_ShouldThrow()
    {
        var board = new BoardBuilder().Build();
        var longName = new string('a', 51);

        var act = () => board.AddLabel(longName, LabelColors.Blue);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.LabelNameTooLong);
    }

    [Fact]
    public void AddLabel_WithInvalidColor_ShouldThrow()
    {
        var board = new BoardBuilder().Build();

        var act = () => board.AddLabel("Bug", "neon-green");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.LabelColorInvalid("neon-green"));
    }

    [Fact]
    public void UpdateLabel_WithValidData_ShouldUpdate()
    {
        var board = new BoardBuilder()
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var labelId = board.Labels[0].Id;

        board.UpdateLabel(labelId, "Feature", LabelColors.Blue);

        board.Labels[0].Name.Should().Be("Feature");
        board.Labels[0].Color.Should().Be(LabelColors.Blue);
    }

    [Fact]
    public void UpdateLabel_ShouldRaiseLabelUpdatedEvent()
    {
        var board = new BoardBuilder()
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var labelId = board.Labels[0].Id;

        board.UpdateLabel(labelId, "Feature", LabelColors.Blue);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<LabelUpdatedEvent>();
    }

    [Fact]
    public void UpdateLabel_WithNonExistentId_ShouldThrow()
    {
        var board = new BoardBuilder().Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.UpdateLabel(fakeId, "Bug", LabelColors.Red);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.LabelNotFound(fakeId));
    }

    [Fact]
    public void RemoveLabel_ShouldRemoveLabelFromBoard()
    {
        var board = new BoardBuilder()
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var labelId = board.Labels[0].Id;

        board.RemoveLabel(labelId);

        board.Labels.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLabel_ShouldRemoveFromAllCards()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .WithCard("To Do", "Card 3")
            .WithLabel("Bug", LabelColors.Red)
            .WithCardLabel(0, 0) // Card 1 has Bug
            .WithCardLabel(1, 0) // Card 2 has Bug
            .Build();
        var labelId = board.Labels[0].Id;

        board.RemoveLabel(labelId);

        board.Cards.Should().OnlyContain(c => c.LabelIds.Count == 0);
    }

    [Fact]
    public void RemoveLabel_ShouldRaiseLabelRemovedEvent()
    {
        var board = new BoardBuilder()
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var labelId = board.Labels[0].Id;

        board.RemoveLabel(labelId);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<LabelRemovedEvent>();
    }

    [Fact]
    public void RemoveLabel_WithNonExistentId_ShouldThrow()
    {
        var board = new BoardBuilder().Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.RemoveLabel(fakeId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.LabelNotFound(fakeId));
    }

    [Fact]
    public void AddLabelToCard_ShouldAssignLabel()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;

        board.AddLabelToCard(cardId, labelId);

        board.Cards[0].LabelIds.Should().ContainSingle().Which.Should().Be(labelId);
    }

    [Fact]
    public void AddLabelToCard_WhenAlreadyAssigned_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .WithCardLabel(0, 0)
            .Build();
        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;

        var act = () => board.AddLabelToCard(cardId, labelId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardLabelAlreadyAssigned);
    }

    [Fact]
    public void AddLabelToCard_WithNonExistentLabel_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .Build();
        var cardId = board.Cards[0].Id;
        var fakeId = Guid.NewGuid();

        var act = () => board.AddLabelToCard(cardId, fakeId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.LabelNotFound(fakeId));
    }

    [Fact]
    public void AddLabelToCard_WithNonExistentCard_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var fakeCardId = Guid.NewGuid();
        var labelId = board.Labels[0].Id;

        var act = () => board.AddLabelToCard(fakeCardId, labelId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotFound(fakeCardId));
    }

    [Fact]
    public void AddLabelToCard_ShouldRaiseCardLabelAddedEvent()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;

        board.AddLabelToCard(cardId, labelId);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CardLabelAddedEvent>();
    }

    [Fact]
    public void RemoveLabelFromCard_ShouldRemoveAssignment()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .WithCardLabel(0, 0)
            .Build();
        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;

        board.RemoveLabelFromCard(cardId, labelId);

        board.Cards[0].LabelIds.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLabelFromCard_WhenNotAssigned_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .Build();
        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;

        var act = () => board.RemoveLabelFromCard(cardId, labelId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardLabelNotAssigned);
    }

    [Fact]
    public void RemoveLabelFromCard_ShouldRaiseCardLabelRemovedEvent()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .WithCardLabel(0, 0)
            .Build();
        var cardId = board.Cards[0].Id;
        var labelId = board.Labels[0].Id;

        board.RemoveLabelFromCard(cardId, labelId);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CardLabelRemovedEvent>();
    }

    [Fact]
    public void MultipleLabelsOnCard_ShouldAllBeTracked()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithLabel("Bug", LabelColors.Red)
            .WithLabel("Urgent", LabelColors.Orange)
            .WithLabel("Backend", LabelColors.Blue)
            .WithCardLabel(0, 0)
            .WithCardLabel(0, 1)
            .WithCardLabel(0, 2)
            .Build();

        board.Cards[0].LabelIds.Should().HaveCount(3);
        board.Cards[0].LabelIds.Should().Contain(board.Labels[0].Id);
        board.Cards[0].LabelIds.Should().Contain(board.Labels[1].Id);
        board.Cards[0].LabelIds.Should().Contain(board.Labels[2].Id);
    }
}
