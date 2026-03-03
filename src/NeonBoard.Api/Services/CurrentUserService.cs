using System.Security.Claims;
using NeonBoard.Api.Constants;
using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public string? Auth0UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(Auth0Claims.Email)?.Value;

    public string? Name =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(Auth0Claims.Name)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public async Task<Guid?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(Auth0UserId))
            return null;

        var email = Email ?? "unknown@neonboard.app";
        var name = Name ?? "Unknown User";

        var user = await _userRepository.GetOrCreateByAuth0IdAsync(Auth0UserId, email, name, cancellationToken);

        return user.Id;
    }
}
