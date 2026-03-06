using Microsoft.EntityFrameworkCore;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Users;
using NeonBoard.Infrastructure.Persistence;

namespace NeonBoard.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByAuth0UserIdAsync(string auth0UserId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Auth0UserId == auth0UserId, cancellationToken);
    }

    public async Task<User> GetOrCreateByAuth0IdAsync(string auth0UserId, string email, string name, CancellationToken cancellationToken = default)
    {
        var user = await GetByAuth0UserIdAsync(auth0UserId, cancellationToken);

        if (user != null)
            return user;

        user = User.Create(auth0UserId, email, name);
        await DbSet.AddAsync(user, cancellationToken);

        return user;
    }
}
