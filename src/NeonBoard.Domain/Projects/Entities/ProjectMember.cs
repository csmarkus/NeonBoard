using NeonBoard.Domain.Common;

namespace NeonBoard.Domain.Projects.Entities;

public sealed class ProjectMember : Entity
{
    public Guid UserId { get; private set; }

    public ProjectRole Role { get; private set; }

    public DateTime JoinedAt { get; private set; }

    private ProjectMember()
    {
    }

    internal static ProjectMember CreateInternal(Guid userId, ProjectRole role)
    {
        if (userId == default)
            throw new DomainException(DomainMessages.MemberUserIdEmpty);

        return new ProjectMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };
    }

    internal void UpdateRole(ProjectRole newRole)
    {
        Role = newRole;
    }
}
