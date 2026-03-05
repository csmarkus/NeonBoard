using System.Security.Claims;
using NeonBoard.Application.Common.Interfaces;

namespace NeonBoard.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly string _emailClaim;
    private readonly string _nameClaim;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;

        var audience = configuration["Auth0:Audience"]
            ?? throw new InvalidOperationException("Auth0:Audience is not configured.");
        _emailClaim = $"{audience}/email";
        _nameClaim = $"{audience}/name";
    }

    public string? Auth0UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(_emailClaim)?.Value;

    public string? Name =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(_nameClaim)?.Value;

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
