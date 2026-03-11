using NeonBoard.Domain.Boards.Entities;
using NeonBoard.Domain.Boards.Events;
using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards;

public sealed class Board : Entity, IAggregateRoot
{
    private const int MaxNameLength = 100;
    private readonly List<Column> _columns = new();
    private readonly List<Card> _cards = new();
    private readonly List<Label> _labels = new();

    public string Name { get; private set; } = default!;

    public string Slug { get; private set; } = default!;

    public Guid ProjectId { get; private set; }

    public IReadOnlyList<Column> Columns => _columns.AsReadOnly();

    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    public IReadOnlyList<Label> Labels => _labels.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public BoardPrefix Prefix { get; private set; } = default!;

    public int NextCardNumber { get; private set; }

    private Board()
    {
    }

    public static Board Create(string name, Guid projectId, string? prefix = null)
    {
        ValidateName(name);
        ValidateProjectId(projectId);

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = SlugHelper.Slugify(name),
            ProjectId = projectId,
            Prefix = prefix != null
                ? BoardPrefix.Create(prefix)
                : BoardPrefix.GenerateFromName(name),
            NextCardNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        board.AddDomainEvent(new BoardCreatedEvent(
            board.Id,
            board.Name,
            board.ProjectId,
            board.CreatedAt));

        return board;
    }

    public void Rename(string newName)
    {
        ValidateName(newName);

        var oldName = Name;
        Name = newName;
        Slug = SlugHelper.Slugify(newName);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BoardRenamedEvent(Id, oldName, newName));
    }

    public void Delete()
    {
        AddDomainEvent(new BoardDeletedEvent(Id, ProjectId, Name));
    }

    public void UpdatePrefix(string newPrefix)
    {
        var oldPrefix = Prefix.Value;
        Prefix = BoardPrefix.Create(newPrefix);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BoardPrefixUpdatedEvent(Id, oldPrefix, newPrefix));
    }

    #region Column Operations

    public Guid AddColumn(string name)
    {
        var lastColumn = _columns
            .OrderByDescending(c => c.Position.Value, StringComparer.Ordinal)
            .FirstOrDefault();
        var position = Position.Between(lastColumn?.Position, null);
        var column = Column.CreateInternal(name, position);

        _columns.Add(column);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ColumnAddedEvent(Id, column.Id, column.Name, column.Position.Value));
        return column.Id;
    }

    public void RenameColumn(Guid columnId, string newName)
    {
        var column = FindColumn(columnId);
        var oldName = column.Name;
        column.UpdateName(newName);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ColumnRenamedEvent(Id, columnId, oldName, newName));
    }

    public void ReorderColumns(List<Guid> columnIdsInOrder)
    {
        if (columnIdsInOrder.Count != _columns.Count)
            throw new DomainException(DomainMessages.BoardColumnCountMismatch);

        foreach (var columnId in columnIdsInOrder)
        {
            if (!_columns.Any(c => c.Id == columnId))
                throw new DomainException(DomainMessages.ColumnNotFound(columnId));
        }

        var keys = FractionalIndex.GenerateNKeysBetween(null, null, columnIdsInOrder.Count);
        var newPositions = new Dictionary<Guid, string>();

        for (int i = 0; i < columnIdsInOrder.Count; i++)
        {
            var column = _columns.First(c => c.Id == columnIdsInOrder[i]);
            column.UpdatePosition(Position.Create(keys[i]));
            newPositions[columnIdsInOrder[i]] = keys[i];
        }

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ColumnsReorderedEvent(Id, newPositions));
    }

    public void MoveColumn(Guid columnId, string newPosition)
    {
        var column = FindColumn(columnId);
        column.UpdatePosition(Position.Create(newPosition));
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ColumnMovedEvent(Id, columnId, newPosition, column.Name));
    }

    public void DeleteColumn(Guid columnId, Guid? moveCardsToColumnId = null)
    {
        var column = FindColumn(columnId);
        var cardsInColumn = GetCardsInColumn(columnId);

        if (cardsInColumn.Count > 0)
        {
            if (moveCardsToColumnId.HasValue)
            {
                var targetColumn = FindColumn(moveCardsToColumnId.Value);
                var lastCardInTarget = GetCardsInColumn(moveCardsToColumnId.Value)
                    .OrderByDescending(c => c.Position.Value, StringComparer.Ordinal)
                    .FirstOrDefault();

                string? prevKey = lastCardInTarget?.Position.Value;
                foreach (var card in cardsInColumn)
                {
                    var newKey = FractionalIndex.GenerateKeyBetween(prevKey, null);
                    card.Move(moveCardsToColumnId.Value, Position.Create(newKey));
                    prevKey = newKey;
                }
            }
            else
            {
                throw new DomainException(DomainMessages.BoardCannotDeleteColumnWithCards);
            }
        }

        _columns.Remove(column);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ColumnDeletedEvent(Id, columnId, moveCardsToColumnId, column.Name));
    }

    #endregion

    #region Card Operations

    public Guid AddCard(Guid columnId, string title, string description)
    {
        var column = FindColumn(columnId);
        var content = CardContent.Create(title, description);

        var lastCard = GetCardsInColumn(columnId)
            .OrderByDescending(c => c.Position.Value, StringComparer.Ordinal)
            .FirstOrDefault();
        var position = Position.Between(lastCard?.Position, null);

        var cardNumber = NextCardNumber;
        NextCardNumber++;

        var card = Card.CreateInternal(columnId, content, position, cardNumber);
        _cards.Add(card);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardCreatedEvent(
            Id, card.Id, columnId, title,
            position.Value, cardNumber,
            column.Name, Prefix.Value));

        return card.Id;
    }

    public void UpdateCard(Guid cardId, string title, string description)
    {
        var card = FindCard(cardId);
        var content = CardContent.Create(title, description);

        card.UpdateContent(content);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardUpdatedEvent(
            Id,
            cardId,
            title,
            description,
            card.CardNumber,
            Prefix.Value));
    }

    public void MoveCard(Guid cardId, Guid targetColumnId, string newPosition)
    {
        var card = FindCard(cardId);
        FindColumn(targetColumnId);

        if (string.IsNullOrWhiteSpace(newPosition))
            throw new DomainException(DomainMessages.PositionEmpty);

        var sourceColumnId = card.ColumnId;
        var sourceColumn = _columns.First(c => c.Id == sourceColumnId);
        var targetColumn = _columns.First(c => c.Id == targetColumnId);

        card.Move(targetColumnId, Position.Create(newPosition));

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardMovedEvent(
            Id, cardId,
            sourceColumnId, targetColumnId,
            newPosition,
            card.Content.Title, card.CardNumber,
            sourceColumn.Name, targetColumn.Name,
            Prefix.Value));
    }

    public void DeleteCard(Guid cardId)
    {
        var card = FindCard(cardId);
        var columnId = card.ColumnId;
        var cardTitle = card.Content.Title;
        var cardNumber = card.CardNumber;

        _cards.Remove(card);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardDeletedEvent(Id, cardId, columnId, cardTitle, cardNumber, Prefix.Value));
    }

    public void ArchiveCard(Guid cardId)
    {
        var card = FindCard(cardId);
        var columnId = card.ColumnId;

        card.Archive();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardArchivedEvent(Id, cardId, columnId,
            card.Content.Title, card.CardNumber, Prefix.Value));
    }

    public void RestoreCard(Guid cardId)
    {
        var card = FindCard(cardId);
        var columnId = card.ColumnId;

        var lastCard = GetCardsInColumn(columnId)
            .OrderByDescending(c => c.Position.Value, StringComparer.Ordinal)
            .FirstOrDefault();
        var restorePosition = Position.Between(lastCard?.Position, null);

        card.Restore();
        card.Move(columnId, restorePosition);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardRestoredEvent(Id, cardId, columnId,
            card.Content.Title, card.CardNumber, Prefix.Value));
    }

    public void HoldCard(Guid cardId)
    {
        var card = FindCard(cardId);

        card.Hold();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardHeldEvent(Id, cardId, card.ColumnId,
            card.Content.Title, card.CardNumber, Prefix.Value));
    }

    public void ResumeCard(Guid cardId)
    {
        var card = FindCard(cardId);

        card.Resume();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardResumedEvent(Id, cardId, card.ColumnId,
            card.Content.Title, card.CardNumber, Prefix.Value));
    }

    #endregion

    #region Label Operations

    public Guid AddLabel(string name, string color)
    {
        var label = Label.Create(name, color);
        _labels.Add(label);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LabelCreatedEvent(Id, label.Id, label.Name, label.Color));

        return label.Id;
    }

    public void UpdateLabel(Guid labelId, string name, string color)
    {
        var label = FindLabel(labelId);
        label.Update(name, color);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LabelUpdatedEvent(Id, labelId, name, color));
    }

    public void RemoveLabel(Guid labelId)
    {
        var label = FindLabel(labelId);
        var labelName = label.Name;

        // Remove label from all cards that have it
        foreach (var card in _cards)
        {
            if (card.LabelIds.Contains(labelId))
            {
                card.RemoveLabel(labelId);
            }
        }

        _labels.Remove(label);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LabelRemovedEvent(Id, labelId, labelName));
    }

    public void AddLabelToCard(Guid cardId, Guid labelId)
    {
        var card = FindCard(cardId);
        var label = FindLabel(labelId);

        card.AddLabel(labelId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardLabelAddedEvent(Id, cardId, labelId,
            card.Content.Title, card.CardNumber, label.Name, label.Color, Prefix.Value));
    }

    public void RemoveLabelFromCard(Guid cardId, Guid labelId)
    {
        var card = FindCard(cardId);
        var label = FindLabel(labelId);

        card.RemoveLabel(labelId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardLabelRemovedEvent(Id, cardId, labelId,
            card.Content.Title, card.CardNumber, label.Name, label.Color, Prefix.Value));
    }

    #endregion

    #region Private Helper Methods

    private Column FindColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column == null)
            throw new DomainException(DomainMessages.ColumnNotFound(columnId));
        return column;
    }

    private Card FindCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card == null)
            throw new DomainException(DomainMessages.CardNotFound(cardId));
        return card;
    }

    private Label FindLabel(Guid labelId)
    {
        var label = _labels.FirstOrDefault(l => l.Id == labelId);
        if (label == null)
            throw new DomainException(DomainMessages.LabelNotFound(labelId));
        return label;
    }

    private List<Card> GetCardsInColumn(Guid columnId)
    {
        return _cards.Where(c => c.ColumnId == columnId && !c.IsArchived).ToList();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainMessages.BoardNameEmpty);

        if (name.Length > MaxNameLength)
            throw new DomainException(DomainMessages.BoardNameTooLong);
    }

    private static void ValidateProjectId(Guid projectId)
    {
        if (projectId == default)
            throw new DomainException(DomainMessages.BoardProjectIdEmpty);
    }

    #endregion
}
