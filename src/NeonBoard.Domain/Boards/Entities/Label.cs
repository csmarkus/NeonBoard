using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Boards.Entities;

public sealed class Label : Entity
{
    private const int MaxNameLength = 50;

    public string Name { get; private set; } = default!;

    public string Color { get; private set; } = default!;

    private Label()
    {
    }

    internal static Label Create(string name, string color)
    {
        ValidateName(name);
        ValidateColor(color);

        return new Label
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color
        };
    }

    internal void Update(string name, string color)
    {
        ValidateName(name);
        ValidateColor(color);

        Name = name;
        Color = color;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainMessages.LabelNameEmpty);

        if (name.Length > MaxNameLength)
            throw new DomainException(DomainMessages.LabelNameTooLong);
    }

    private static void ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException(DomainMessages.LabelColorEmpty);

        if (!LabelColors.IsValid(color))
            throw new DomainException(DomainMessages.LabelColorInvalid(color));
    }
}
