using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Users.Events;

public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime CreatedAt) : IDomainEvent;
