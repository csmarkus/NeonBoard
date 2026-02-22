using System.Text.RegularExpressions;

namespace NeonBoard.Domain.Common;

public static partial class SlugHelper
{
    public static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Cannot create slug from empty string.");

        var slug = input.ToLowerInvariant().Trim();
        slug = NonAlphanumericRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleHyphensRegex();
}
