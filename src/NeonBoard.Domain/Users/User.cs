using NeonBoard.Domain.Common;
using NeonBoard.Domain.Users.Events;

namespace NeonBoard.Domain.Users;

public sealed class User : Entity, IAggregateRoot
{
    private const int MaxEmailLength = 254;
    private const int MaxDisplayNameLength = 100;
    private const int MaxAuth0UserIdLength = 100;

    public string Auth0UserId { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public string DisplayName { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(string auth0UserId, string email, string displayName)
    {
        ValidateAuth0UserId(auth0UserId);
        ValidateEmail(email);
        ValidateDisplayName(displayName);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Auth0UserId = auth0UserId,
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

    private static void ValidateAuth0UserId(string auth0UserId)
    {
        if (string.IsNullOrWhiteSpace(auth0UserId))
            throw new DomainException("Auth0 User ID cannot be empty.");

        if (auth0UserId.Length > MaxAuth0UserIdLength)
            throw new DomainException($"Auth0 User ID cannot exceed {MaxAuth0UserIdLength} characters.");
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
