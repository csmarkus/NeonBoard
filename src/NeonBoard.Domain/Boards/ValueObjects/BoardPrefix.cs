using System.Text.RegularExpressions;
using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.ValueObjects;

public sealed partial class BoardPrefix : ValueObject
{
    private const int MinLength = 2;
    private const int MaxLength = 5;

    public string Value { get; init; } = default!;

    private BoardPrefix()
    {
    }

    public static BoardPrefix Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainMessages.BoardPrefixEmpty);

        if (!PrefixRegex().IsMatch(value))
            throw new DomainException(DomainMessages.BoardPrefixInvalid);

        return new BoardPrefix { Value = value };
    }

    public static BoardPrefix GenerateFromName(string boardName)
    {
        if (string.IsNullOrWhiteSpace(boardName))
            throw new DomainException(DomainMessages.BoardNameEmpty);

        var words = boardName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string prefix;

        if (words.Length == 1)
        {
            var clean = new string(words[0].Where(char.IsLetter).ToArray());
            if (clean.Length == 0)
                throw new DomainException(DomainMessages.BoardPrefixCannotBeGenerated);

            prefix = clean.Length >= 3
                ? clean[..3].ToUpperInvariant()
                : clean.ToUpperInvariant().PadRight(MinLength, clean.ToUpperInvariant().Last());
        }
        else
        {
            var initials = words
                .Select(w => w.Where(char.IsLetter).FirstOrDefault())
                .Where(c => c != default)
                .Take(MaxLength)
                .Select(char.ToUpperInvariant);
            prefix = new string(initials.ToArray());
        }

        if (prefix.Length == 0)
            throw new DomainException(DomainMessages.BoardPrefixCannotBeGenerated);

        if (prefix.Length < MinLength)
            prefix = prefix.PadRight(MinLength, prefix.Last());

        if (prefix.Length > MaxLength)
            prefix = prefix[..MaxLength];

        return new BoardPrefix { Value = prefix };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[A-Z]{2,5}$")]
    private static partial Regex PrefixRegex();
}
