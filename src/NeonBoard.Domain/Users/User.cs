using NeonBoard.Domain.Common;
using NeonBoard.Domain.Users.Events;

namespace NeonBoard.Domain.Users;

public sealed class User : Entity, IAggregateRoot
{
    private const int MaxEmailLength = 254;
    private const int MaxDisplayNameLength = 100;

    public string Email { get; private set; } = default!;

    public string DisplayName { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(string email, string displayName)
    {
        ValidateEmail(email);
        ValidateDisplayName(displayName);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(
            user.Id,
            user.Email,
            user.DisplayName,
            user.CreatedAt));

        return user;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        if (email.Length > MaxEmailLength)
            throw new DomainException($"Email cannot exceed {MaxEmailLength} characters.");

        if (!email.Contains('@'))
            throw new DomainException("Email must contain '@' symbol.");
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name cannot be empty.");

        if (displayName.Length > MaxDisplayNameLength)
            throw new DomainException($"Display name cannot exceed {MaxDisplayNameLength} characters.");
    }
}
