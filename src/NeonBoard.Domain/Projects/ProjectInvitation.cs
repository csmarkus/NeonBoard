using System.Security.Cryptography;
using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects.Events;

namespace NeonBoard.Domain.Projects;

public sealed class ProjectInvitation : Entity, IAggregateRoot
{
    private const int TokenLength = 32;

    public Guid ProjectId { get; private set; }
    public string Email { get; private set; } = default!;
    public ProjectRole Role { get; private set; }
    public string Token { get; private set; } = default!;
    public InvitationStatus Status { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProjectInvitation()
    {
    }

    public static ProjectInvitation Create(
        Guid projectId,
        string email,
        ProjectRole role,
        Guid invitedByUserId,
        DateTime expiresAt)
    {
        if (projectId == default)
            throw new DomainException(DomainMessages.InvitationProjectIdEmpty);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(DomainMessages.InvitationEmailEmpty);

        if (!email.Contains('@'))
            throw new DomainException(DomainMessages.InvitationEmailInvalid);

        if (invitedByUserId == default)
            throw new DomainException(DomainMessages.InvitationInviterIdEmpty);

        var invitation = new ProjectInvitation
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Token = GenerateToken(),
            Status = InvitationStatus.Pending,
            InvitedByUserId = invitedByUserId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        invitation.AddDomainEvent(new InvitationCreatedEvent(
            invitation.Id,
            invitation.ProjectId,
            invitation.Email,
            invitation.Role,
            invitation.InvitedByUserId,
            DateTime.UtcNow));

        return invitation;
    }

    public void Accept(Guid userId)
    {
        if (Status == InvitationStatus.Accepted)
            throw new DomainException(DomainMessages.InvitationAlreadyAccepted);

        if (Status == InvitationStatus.Revoked)
            throw new DomainException(DomainMessages.InvitationAlreadyRevoked);

        if (Status != InvitationStatus.Pending)
            throw new DomainException(DomainMessages.InvitationNotPending);

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = InvitationStatus.Expired;
            throw new DomainException(DomainMessages.InvitationExpired);
        }

        Status = InvitationStatus.Accepted;
        AcceptedByUserId = userId;

        AddDomainEvent(new InvitationAcceptedEvent(
            Id, ProjectId, userId, Role, DateTime.UtcNow));
    }

    public void Revoke()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException(DomainMessages.InvitationNotPending);

        Status = InvitationStatus.Revoked;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenLength);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
