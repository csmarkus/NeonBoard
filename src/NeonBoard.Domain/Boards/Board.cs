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

    public string Name { get; private set; } = default!;

    public Guid ProjectId { get; private set; }

    public IReadOnlyList<Column> Columns => _columns.AsReadOnly();

    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Board()
    {
    }

    public static Board Create(string name, Guid projectId)
    {
        ValidateName(name);
        ValidateProjectId(projectId);

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectId = projectId,
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

        Name = newName;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BoardRenamedEvent(Id, newName));
    }

    #region Column Operations

    public Guid AddColumn(string name)
    {
        var position = Position.Create(_columns.Count);
        var column = Column.CreateInternal(name, position);

        _columns.Add(column);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ColumnAddedEvent(
            Id,
            column.Id,
            column.Name,
            column.Position.Value));

        return column.Id;
    }

    public void RenameColumn(Guid columnId, string newName)
    {
        var column = FindColumn(columnId);
        column.UpdateName(newName);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderColumns(List<Guid> columnIdsInOrder)
    {
        if (columnIdsInOrder.Count != _columns.Count)
            throw new DomainException("Column count mismatch. All columns must be included in the reorder.");

        foreach (var columnId in columnIdsInOrder)
        {
            if (!_columns.Any(c => c.Id == columnId))
                throw new DomainException($"Column with ID {columnId} not found.");
        }

        var newPositions = new Dictionary<Guid, int>();
        for (int i = 0; i < columnIdsInOrder.Count; i++)
        {
            var column = _columns.First(c => c.Id == columnIdsInOrder[i]);
            column.UpdatePosition(Position.Create(i));
            newPositions[columnIdsInOrder[i]] = i;
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ColumnsReorderedEvent(Id, newPositions));
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
                var targetColumnCardCount = GetCardsInColumn(moveCardsToColumnId.Value).Count;

                foreach (var card in cardsInColumn)
                {
                    card.Move(moveCardsToColumnId.Value, Position.Create(targetColumnCardCount));
                    targetColumnCardCount++;
                }

                ResequenceCardsInColumn(moveCardsToColumnId.Value);
            }
            else
            {
                throw new DomainException("Cannot delete column with cards. Specify a target column to move cards to.");
            }
        }

        _columns.Remove(column);
        ResequenceColumns();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ColumnDeletedEvent(Id, columnId, moveCardsToColumnId));
    }

    #endregion

    #region Card Operations

    public Guid AddCard(Guid columnId, string title, string description)
    {
        var column = FindColumn(columnId);
        var content = CardContent.Create(title, description);
        var cardsInColumn = GetCardsInColumn(columnId);
        var position = Position.Create(cardsInColumn.Count);

        var card = Card.CreateInternal(columnId, content, position);
        _cards.Add(card);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardCreatedEvent(
            Id,
            card.Id,
            columnId,
            title,
            position.Value));

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
            description));
    }

    public void MoveCard(Guid cardId, Guid targetColumnId, int targetPosition)
    {
        var card = FindCard(cardId);
        var targetColumn = FindColumn(targetColumnId);

        if (targetPosition < 0)
            throw new DomainException("Target position cannot be negative.");

        var sourceColumnId = card.ColumnId;

        card.Move(targetColumnId, Position.Create(targetPosition));

        ResequenceCardsInColumn(sourceColumnId);
        if (sourceColumnId != targetColumnId)
        {
            ResequenceCardsInColumn(targetColumnId);
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardMovedEvent(
            Id,
            cardId,
            sourceColumnId,
            targetColumnId,
            targetPosition));
    }

    public void DeleteCard(Guid cardId)
    {
        var card = FindCard(cardId);
        var columnId = card.ColumnId;

        _cards.Remove(card);
        ResequenceCardsInColumn(columnId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CardDeletedEvent(Id, cardId, columnId));
    }

    #endregion

    #region Private Helper Methods

    private Column FindColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column == null)
            throw new DomainException($"Column with ID {columnId} not found.");
        return column;
    }

    private Card FindCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card == null)
            throw new DomainException($"Card with ID {cardId} not found.");
        return card;
    }

    private List<Card> GetCardsInColumn(Guid columnId)
    {
        return _cards.Where(c => c.ColumnId == columnId).ToList();
    }

    private void ResequenceCardsInColumn(Guid columnId)
    {
        var cardsInColumn = GetCardsInColumn(columnId)
            .OrderBy(c => c.Position.Value)
            .ToList();

        for (int i = 0; i < cardsInColumn.Count; i++)
        {
            cardsInColumn[i].Move(columnId, Position.Create(i));
        }
    }

    private void ResequenceColumns()
    {
        var orderedColumns = _columns.OrderBy(c => c.Position.Value).ToList();

        for (int i = 0; i < orderedColumns.Count; i++)
        {
            orderedColumns[i].UpdatePosition(Position.Create(i));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Board name cannot be empty.");

        if (name.Length > MaxNameLength)
            throw new DomainException($"Board name cannot exceed {MaxNameLength} characters.");
    }

    private static void ValidateProjectId(Guid projectId)
    {
        if (projectId == default)
            throw new DomainException("Project ID cannot be empty.");
    }

    #endregion
}
