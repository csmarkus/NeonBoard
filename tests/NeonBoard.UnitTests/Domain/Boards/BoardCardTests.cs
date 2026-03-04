using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Common;
using NeonBoard.UnitTests.Builders;

namespace NeonBoard.UnitTests.Domain.Boards;

public class BoardCardTests
{
    [Fact]
    public void AddCard_ToExistingColumn_ShouldAddCard()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;

        var cardId = board.AddCard(columnId, "My Card", "Description");

        board.Cards.Should().HaveCount(1);
        var card = board.Cards[0];
        card.Id.Should().Be(cardId);
        card.ColumnId.Should().Be(columnId);
        card.Content.Title.Should().Be("My Card");
        card.Content.Description.Should().Be("Description");
        card.Position.Value.Should().Be(0);
    }

    [Fact]
    public void AddCard_ShouldAssignSequentialPositions()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;

        board.AddCard(columnId, "Card 1", "");
        board.AddCard(columnId, "Card 2", "");
        board.AddCard(columnId, "Card 3", "");

        board.Cards[0].Position.Value.Should().Be(0);
        board.Cards[1].Position.Value.Should().Be(1);
        board.Cards[2].Position.Value.Should().Be(2);
    }

    [Fact]
    public void AddCard_ToNonExistentColumn_ShouldThrow()
    {
        var board = new BoardBuilder().Build();
        var fakeColumnId = Guid.NewGuid();

        var act = () => board.AddCard(fakeColumnId, "Card", "");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ColumnNotFound(fakeColumnId));
    }

    [Fact]
    public void AddCard_ShouldRaiseCardCreatedEvent()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;

        board.AddCard(columnId, "My Card", "Desc");

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CardCreatedEvent>();
    }

    [Fact]
    public void UpdateCard_ShouldUpdateContent()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Old Title", "Old Desc")
            .Build();
        var cardId = board.Cards[0].Id;

        board.UpdateCard(cardId, "New Title", "New Desc");

        board.Cards[0].Content.Title.Should().Be("New Title");
        board.Cards[0].Content.Description.Should().Be("New Desc");
    }

    [Fact]
    public void UpdateCard_WithNonExistentId_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.UpdateCard(fakeId, "Title", "Desc");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotFound(fakeId));
    }

    [Fact]
    public void UpdateCard_ShouldRaiseCardUpdatedEvent()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Title", "Desc")
            .Build();
        var cardId = board.Cards[0].Id;

        board.UpdateCard(cardId, "New", "New");

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CardUpdatedEvent>();
    }

    [Fact]
    public void MoveCard_ToDifferentColumn_ShouldUpdateColumnAndPosition()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .WithCard("To Do", "Card 1")
            .Build();
        var cardId = board.Cards[0].Id;
        var targetColumnId = board.Columns[1].Id;

        board.MoveCard(cardId, targetColumnId, 0);

        board.Cards[0].ColumnId.Should().Be(targetColumnId);
        board.Cards[0].Position.Value.Should().Be(0);
    }

    [Fact]
    public void MoveCard_ShouldResequenceSourceAndTargetColumns()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .WithCard("Done", "Card 3")
            .Build();
        var cardToMove = board.Cards[0]; // Card 1
        var targetColumnId = board.Columns[1].Id;

        board.MoveCard(cardToMove.Id, targetColumnId, 0);

        // Remaining card in "To Do" should be resequenced to position 0
        var toDoCards = board.Cards.Where(c => c.ColumnId == board.Columns[0].Id).ToList();
        toDoCards.Should().HaveCount(1);
        toDoCards[0].Position.Value.Should().Be(0);
    }

    [Fact]
    public void MoveCard_WithNegativePosition_ShouldThrow()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .Build();
        var cardId = board.Cards[0].Id;
        var columnId = board.Columns[0].Id;

        var act = () => board.MoveCard(cardId, columnId, -1);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.BoardTargetPositionNegative);
    }

    [Fact]
    public void MoveCard_ShouldRaiseCardMovedEvent()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .WithCard("To Do", "Card 1")
            .Build();
        var cardId = board.Cards[0].Id;
        var sourceColumnId = board.Columns[0].Id;
        var targetColumnId = board.Columns[1].Id;

        board.MoveCard(cardId, targetColumnId, 0);

        var domainEvent = board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CardMovedEvent>().Subject;

        domainEvent.CardId.Should().Be(cardId);
        domainEvent.SourceColumnId.Should().Be(sourceColumnId);
        domainEvent.TargetColumnId.Should().Be(targetColumnId);
    }

    [Fact]
    public void DeleteCard_ShouldRemoveCard()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .Build();
        var cardId = board.Cards[0].Id;

        board.DeleteCard(cardId);

        board.Cards.Should().BeEmpty();
    }

    [Fact]
    public void DeleteCard_ShouldResequenceRemainingCards()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .WithCard("To Do", "Card 3")
            .Build();
        var middleCardId = board.Cards[1].Id;

        board.DeleteCard(middleCardId);

        board.Cards.Should().HaveCount(2);
        board.Cards.OrderBy(c => c.Position.Value).First().Position.Value.Should().Be(0);
        board.Cards.OrderBy(c => c.Position.Value).Last().Position.Value.Should().Be(1);
    }

    [Fact]
    public void DeleteCard_ShouldRaiseCardDeletedEvent()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .Build();
        var cardId = board.Cards[0].Id;

        board.DeleteCard(cardId);

        board.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CardDeletedEvent>();
    }

    [Fact]
    public void DeleteCard_WithNonExistentId_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.DeleteCard(fakeId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotFound(fakeId));
    }

    [Fact]
    public void MoveCard_ToFirstPositionInSameColumn_ShouldPlaceCardAtPositionZero()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card A")
            .WithCard("To Do", "Card B")
            .WithCard("To Do", "Card C")
            .Build();
        var columnId = board.Columns[0].Id;
        var cardC = board.Cards.Single(c => c.Content.Title == "Card C");

        board.MoveCard(cardC.Id, columnId, 0);

        var orderedCards = board.Cards.OrderBy(c => c.Position.Value).ToList();
        orderedCards[0].Content.Title.Should().Be("Card C");
        orderedCards[0].Position.Value.Should().Be(0);
        orderedCards[1].Content.Title.Should().Be("Card A");
        orderedCards[1].Position.Value.Should().Be(1);
        orderedCards[2].Content.Title.Should().Be("Card B");
        orderedCards[2].Position.Value.Should().Be(2);
    }

    [Fact]
    public void MoveCard_ToFirstPositionInDifferentColumn_ShouldPlaceCardAtPositionZero()
    {
        var board = new BoardBuilder()
            .WithColumns("To Do", "Done")
            .WithCard("To Do", "Card A")
            .WithCard("Done", "Card B")
            .WithCard("Done", "Card C")
            .Build();
        var targetColumnId = board.Columns[1].Id;
        var cardA = board.Cards.Single(c => c.Content.Title == "Card A");

        board.MoveCard(cardA.Id, targetColumnId, 0);

        var doneCards = board.Cards.Where(c => c.ColumnId == targetColumnId)
            .OrderBy(c => c.Position.Value).ToList();
        doneCards[0].Content.Title.Should().Be("Card A");
        doneCards[0].Position.Value.Should().Be(0);
        doneCards[1].Position.Value.Should().Be(1);
        doneCards[2].Position.Value.Should().Be(2);
    }

    [Fact]
    public void AddCard_ShouldAssignCardNumber()
    {
        var board = new BoardBuilder().WithColumn("Todo").Build();
        var columnId = board.Columns.First().Id;

        var cardId = board.AddCard(columnId, "First Card", "");

        var card = board.Cards.First(c => c.Id == cardId);
        card.CardNumber.Should().Be(1);
    }

    [Fact]
    public void AddCard_ShouldIncrementNextCardNumber()
    {
        var board = new BoardBuilder().WithColumn("Todo").Build();
        var columnId = board.Columns.First().Id;

        board.AddCard(columnId, "Card 1", "");
        board.AddCard(columnId, "Card 2", "");

        board.NextCardNumber.Should().Be(3);
        board.Cards.First().CardNumber.Should().Be(1);
        board.Cards.Last().CardNumber.Should().Be(2);
    }

    [Fact]
    public void AddCard_AfterDeletion_ShouldNotReuseCardNumber()
    {
        var board = new BoardBuilder().WithColumn("Todo").Build();
        var columnId = board.Columns.First().Id;

        var cardId1 = board.AddCard(columnId, "Card 1", "");
        board.DeleteCard(cardId1);
        var cardId2 = board.AddCard(columnId, "Card 2", "");

        var card2 = board.Cards.First(c => c.Id == cardId2);
        card2.CardNumber.Should().Be(2);
        board.NextCardNumber.Should().Be(3);
    }
    [Fact]
    public void ArchiveCard_ShouldMarkCardAsArchived()
    {
        var board = new BoardBuilder().WithColumn("To Do").WithCard("To Do", "My Card").Build();
        var cardId = board.Cards[0].Id;

        board.ArchiveCard(cardId);

        board.Cards.First(c => c.Id == cardId).IsArchived.Should().BeTrue();
    }

    [Fact]
    public void ArchiveCard_ShouldResequenceRemainingActiveCards()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .WithCard("To Do", "Card 3")
            .Build();
        var card2Id = board.Cards[1].Id;

        board.ArchiveCard(card2Id);

        var activeCards = board.Cards.Where(c => !c.IsArchived).OrderBy(c => c.Position.Value).ToList();
        activeCards.Should().HaveCount(2);
        activeCards[0].Position.Value.Should().Be(0);
        activeCards[1].Position.Value.Should().Be(1);
    }

    [Fact]
    public void ArchiveCard_ShouldRaiseCardArchivedEvent()
    {
        var board = new BoardBuilder().WithColumn("To Do").WithCard("To Do", "My Card").Build();
        var cardId = board.Cards[0].Id;
        var columnId = board.Cards[0].ColumnId;

        board.ArchiveCard(cardId);

        var evt = board.GetDomainEvents().OfType<CardArchivedEvent>().Single();
        evt.CardId.Should().Be(cardId);
        evt.BoardId.Should().Be(board.Id);
        evt.ColumnId.Should().Be(columnId);
    }

    [Fact]
    public void ArchiveCard_AlreadyArchived_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").WithCard("To Do", "My Card").Build();
        var cardId = board.Cards[0].Id;
        board.ArchiveCard(cardId);
        board.ClearDomainEvents();

        var act = () => board.ArchiveCard(cardId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardAlreadyArchived);
    }

    [Fact]
    public void ArchiveCard_NonExistentCard_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.ArchiveCard(fakeId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotFound(fakeId));
    }

    [Fact]
    public void RestoreCard_ShouldClearArchivedAt_AndPlaceAtEndOfColumn()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .Build();
        var card1Id = board.Cards[0].Id;
        var columnId = board.Cards[0].ColumnId;
        board.ArchiveCard(card1Id);
        board.ClearDomainEvents();

        board.RestoreCard(card1Id);

        var card1 = board.Cards.First(c => c.Id == card1Id);
        card1.IsArchived.Should().BeFalse();
        card1.ColumnId.Should().Be(columnId);
        // Card 2 was resequenced to position 0 after archive; restored card goes to end (position 1)
        card1.Position.Value.Should().Be(1);
    }

    [Fact]
    public void RestoreCard_ShouldRaiseCardRestoredEvent()
    {
        var board = new BoardBuilder().WithColumn("To Do").WithCard("To Do", "My Card").Build();
        var cardId = board.Cards[0].Id;
        var columnId = board.Cards[0].ColumnId;
        board.ArchiveCard(cardId);
        board.ClearDomainEvents();

        board.RestoreCard(cardId);

        var evt = board.GetDomainEvents().OfType<CardRestoredEvent>().Single();
        evt.CardId.Should().Be(cardId);
        evt.BoardId.Should().Be(board.Id);
        evt.ColumnId.Should().Be(columnId);
    }

    [Fact]
    public void RestoreCard_NotArchived_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").WithCard("To Do", "My Card").Build();
        var cardId = board.Cards[0].Id;

        var act = () => board.RestoreCard(cardId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotArchived);
    }

    [Fact]
    public void AddCard_ShouldNotCountArchivedCardsForPosition()
    {
        var board = new BoardBuilder()
            .WithColumn("To Do")
            .WithCard("To Do", "Card 1")
            .WithCard("To Do", "Card 2")
            .Build();
        var columnId = board.Columns[0].Id;
        board.ArchiveCard(board.Cards[0].Id);
        board.ClearDomainEvents();

        board.AddCard(columnId, "Card 3", "");

        var newCard = board.Cards.OrderByDescending(c => c.CreatedAt).First();
        // Only 1 active card in column (Card 2 at pos 0), new card goes to position 1
        newCard.Position.Value.Should().Be(1);
    }

    [Fact]
    public void RestoreCard_NonExistentCard_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.RestoreCard(fakeId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotFound(fakeId));
    }

    [Fact]
    public void HoldCard_ShouldMarkCardAsOnHold()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;
        var cardId = board.AddCard(columnId, "Card", "");

        board.HoldCard(cardId);

        board.Cards.First(c => c.Id == cardId).IsOnHold.Should().BeTrue();
    }

    [Fact]
    public void HoldCard_ShouldNotResequenceCards()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;
        var card1Id = board.AddCard(columnId, "Card 1", "");
        var card2Id = board.AddCard(columnId, "Card 2", "");
        var card3Id = board.AddCard(columnId, "Card 3", "");

        board.HoldCard(card2Id);

        var cards = board.Cards.OrderBy(c => c.Position.Value).ToList();
        cards[0].Position.Value.Should().Be(0);
        cards[1].Position.Value.Should().Be(1);
        cards[2].Position.Value.Should().Be(2);
    }

    [Fact]
    public void HoldCard_ShouldRaiseCardHeldEvent()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;
        var cardId = board.AddCard(columnId, "Card", "");
        board.ClearDomainEvents();

        board.HoldCard(cardId);

        var evt = board.GetDomainEvents().OfType<CardHeldEvent>().Single();
        evt.CardId.Should().Be(cardId);
    }

    [Fact]
    public void HoldCard_AlreadyOnHold_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var cardId = board.AddCard(board.Columns[0].Id, "Card", "");
        board.HoldCard(cardId);

        var act = () => board.HoldCard(cardId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardAlreadyOnHold);
    }

    [Fact]
    public void HoldCard_NonExistentCard_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.HoldCard(fakeId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ResumeCard_ShouldClearHoldAt()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var columnId = board.Columns[0].Id;
        var cardId = board.AddCard(columnId, "Card", "");
        board.HoldCard(cardId);

        board.ResumeCard(cardId);

        var card = board.Cards.First(c => c.Id == cardId);
        card.IsOnHold.Should().BeFalse();
    }

    [Fact]
    public void ResumeCard_ShouldRaiseCardResumedEvent()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var cardId = board.AddCard(board.Columns[0].Id, "Card", "");
        board.HoldCard(cardId);
        board.ClearDomainEvents();

        board.ResumeCard(cardId);

        var evt = board.GetDomainEvents().OfType<CardResumedEvent>().Single();
        evt.CardId.Should().Be(cardId);
    }

    [Fact]
    public void ResumeCard_NotOnHold_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var cardId = board.AddCard(board.Columns[0].Id, "Card", "");

        var act = () => board.ResumeCard(cardId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CardNotOnHold);
    }

    [Fact]
    public void ResumeCard_NonExistentCard_ShouldThrow()
    {
        var board = new BoardBuilder().WithColumn("To Do").Build();
        var fakeId = Guid.NewGuid();

        var act = () => board.ResumeCard(fakeId);

        act.Should().Throw<DomainException>();
    }
}
