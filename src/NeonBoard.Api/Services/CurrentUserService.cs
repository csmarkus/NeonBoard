using System.Security.Claims;
using NeonBoard.Api.Constants;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Users;

namespace NeonBoard.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
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

        var user = await _userRepository.GetByAuth0UserIdAsync(Auth0UserId, cancellationToken);

        if (user != null)
            return user.Id;

        var email = Email ?? "unknown@neonboard.app";
        var name = Name ?? "Unknown User";

        user = User.Create(Auth0UserId, email, name);
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
