using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.ValueObjects;

public sealed class Position : ValueObject
{
    public int Value { get; init; }

    private Position()
    {
    }

    public static Position Create(int value)
    {
        if (value < 0)
            throw new DomainException("Position cannot be negative.");

        return new Position { Value = value };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
