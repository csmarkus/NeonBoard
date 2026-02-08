namespace NeonBoard.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Task<Guid?> GetUserIdAsync(CancellationToken cancellationToken = default);
    string? Auth0UserId { get; }
    string? Email { get; }
    string? Name { get; }
    bool IsAuthenticated { get; }
}
