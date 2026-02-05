using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.ValueObjects;

public sealed class CardContent : ValueObject
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 5000;

    public string Title { get; init; } = default!;

    public string Description { get; init; } = default!;

    private CardContent()
    {
    }

    public static CardContent Create(string title, string? description)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        return new CardContent
        {
            Title = title,
            Description = description ?? string.Empty
        };
    }

    public CardContent Update(string title, string? description)
    {
        return Create(title, description);
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Card title cannot be empty.");

        if (title.Length > MaxTitleLength)
            throw new DomainException($"Card title cannot exceed {MaxTitleLength} characters.");
    }

    private static void ValidateDescription(string? description)
    {
        if (description != null && description.Length > MaxDescriptionLength)
            throw new DomainException($"Card description cannot exceed {MaxDescriptionLength} characters.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return Description;
    }
}
