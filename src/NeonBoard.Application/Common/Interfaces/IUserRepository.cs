using NeonBoard.Domain.Users;

namespace NeonBoard.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByAuth0UserIdAsync(string auth0UserId, CancellationToken cancellationToken = default);
    Task<User> GetOrCreateByAuth0IdAsync(string auth0UserId, string email, string name, CancellationToken cancellationToken = default);
}
