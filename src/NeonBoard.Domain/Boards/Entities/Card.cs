using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Entities;

public sealed class Card : Entity
{
    public Guid ColumnId { get; private set; }

    public CardContent Content { get; private set; } = default!;

    public Position Position { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Card()
    {
    }

    internal static Card CreateInternal(Guid columnId, CardContent content, Position position)
    {
        if (columnId == default)
            throw new DomainException("Column ID cannot be empty.");

        return new Card
        {
            Id = Guid.NewGuid(),
            ColumnId = columnId,
            Content = content,
            Position = position,
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
}
