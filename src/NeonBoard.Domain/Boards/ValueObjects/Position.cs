using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.ValueObjects;

public sealed class Position : ValueObject
{
    public string Value { get; init; } = default!;

    private Position() { }

    public static Position Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainMessages.PositionEmpty);

        return new Position { Value = value };
    }

    public static Position Initial()
    {
        return new Position { Value = FractionalIndex.GenerateKeyBetween(null, null) };
    }

    public static Position Between(Position? before, Position? after)
    {
        var key = FractionalIndex.GenerateKeyBetween(before?.Value, after?.Value);
        return new Position { Value = key };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
