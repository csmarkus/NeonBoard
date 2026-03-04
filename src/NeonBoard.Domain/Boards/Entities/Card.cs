using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Entities;

public sealed class Card : Entity
{
    public Guid ColumnId { get; private set; }

    public CardContent Content { get; private set; } = default!;

    public Position Position { get; private set; } = default!;

    public int CardNumber { get; private set; }

    public List<Guid> LabelIds { get; private set; } = new();

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public bool IsArchived => ArchivedAt.HasValue;

    public DateTime? HoldAt { get; private set; }

    public bool IsOnHold => HoldAt.HasValue;

    private Card()
    {
    }

    internal static Card CreateInternal(Guid columnId, CardContent content, Position position, int cardNumber)
    {
        if (columnId == default)
            throw new DomainException(DomainMessages.CardColumnIdEmpty);

        if (cardNumber <= 0)
            throw new DomainException(DomainMessages.CardNumberInvalid);

        return new Card
        {
            Id = Guid.NewGuid(),
            ColumnId = columnId,
            Content = content,
            Position = position,
            CardNumber = cardNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    internal void UpdateContent(CardContent content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Move(Guid columnId, Position position)
    {
        ColumnId = columnId;
        Position = position;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void AddLabel(Guid labelId)
    {
        if (LabelIds.Contains(labelId))
            throw new DomainException(DomainMessages.CardLabelAlreadyAssigned);

        LabelIds = [..LabelIds, labelId];
        UpdatedAt = DateTime.UtcNow;
    }

    internal void RemoveLabel(Guid labelId)
    {
        if (!LabelIds.Contains(labelId))
            throw new DomainException(DomainMessages.CardLabelNotAssigned);

        LabelIds = LabelIds.Where(id => id != labelId).ToList();
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Archive()
    {
        if (IsArchived)
            throw new DomainException(DomainMessages.CardAlreadyArchived);

        ArchivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Restore()
    {
        if (!IsArchived)
            throw new DomainException(DomainMessages.CardNotArchived);

        ArchivedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Hold()
    {
        if (IsOnHold)
            throw new DomainException(DomainMessages.CardAlreadyOnHold);

        HoldAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Resume()
    {
        if (!IsOnHold)
            throw new DomainException(DomainMessages.CardNotOnHold);

        HoldAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
