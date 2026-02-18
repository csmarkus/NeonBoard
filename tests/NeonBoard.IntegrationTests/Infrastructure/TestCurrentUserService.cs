using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.IntegrationTests.Infrastructure;

public class TestCurrentUserService : ICurrentUserService
{
    private Guid? _userId;

    public void SetUserId(Guid userId) => _userId = userId;

    public Task<Guid?> GetUserIdAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_userId);

    public string? Auth0UserId => TestAuthHandler.TestAuth0UserId;
    public string? Email => TestAuthHandler.TestEmail;
    public string? Name => TestAuthHandler.TestName;
    public bool IsAuthenticated => _userId.HasValue;
}
