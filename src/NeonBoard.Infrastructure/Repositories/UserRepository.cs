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
}
