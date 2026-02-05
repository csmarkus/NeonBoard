using NeonBoard.Domain.Boards.ValueObjects;
using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Entities;

public sealed class Column : Entity
{
    private const int MaxNameLength = 50;

    public string Name { get; private set; } = default!;

    public Position Position { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    private Column()
    {
    }

    internal static Column CreateInternal(string name, Position position)
    {
        ValidateName(name);

        return new Column
        {
            Id = Guid.NewGuid(),
            Name = name,
            Position = position,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    internal void UpdatePosition(Position position)
    {
        Position = position;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Column name cannot be empty.");

        if (name.Length > MaxNameLength)
            throw new DomainException($"Column name cannot exceed {MaxNameLength} characters.");
    }
}
