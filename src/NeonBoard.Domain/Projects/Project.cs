using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects.Entities;
using NeonBoard.Domain.Projects.Events;

namespace NeonBoard.Domain.Projects;

public sealed class Project : Entity, IAggregateRoot
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 1000;

    private readonly List<ProjectMember> _members = new();

    public string Name { get; private set; } = default!;

    public string ShortId { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public Guid OwnerId { get; private set; }

    public IReadOnlyList<ProjectMember> Members => _members.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Project()
    {
    }

    public static Project Create(string name, string description, Guid ownerId)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidateOwnerId(ownerId);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            ShortId = Url62.Generate(7, unambiguous: true),
            Name = name,
            Description = description ?? string.Empty,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        project._members.Add(ProjectMember.CreateInternal(ownerId, ProjectRole.Owner));

        project.AddDomainEvent(new ProjectCreatedEvent(
            project.Id,
            project.Name,
            project.OwnerId,
            project.CreatedAt));

        return project;
    }

    public void Update(string name, string description)
    {
        ValidateName(name);
        ValidateDescription(description);

        Name = name;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectUpdatedEvent(Id, Name, Description));
    }

    #region Membership Operations

    public ProjectMember AddMember(Guid userId, ProjectRole role)
    {
        if (userId == default)
            throw new DomainException(DomainMessages.MemberUserIdEmpty);

        if (_members.Any(m => m.UserId == userId))
            throw new DomainException(DomainMessages.MemberAlreadyExists);

        var member = ProjectMember.CreateInternal(userId, role);
        _members.Add(member);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberAddedToProjectEvent(Id, userId, role, DateTime.UtcNow));

        return member;
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new DomainException(DomainMessages.MemberNotFoundByUserId(userId));

        if (member.Role == ProjectRole.Owner && _members.Count(m => m.Role == ProjectRole.Owner) <= 1)
            throw new DomainException(DomainMessages.CannotRemoveLastOwner);

        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberRemovedFromProjectEvent(Id, userId, DateTime.UtcNow));
    }

    public void UpdateMemberRole(Guid userId, ProjectRole newRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new DomainException(DomainMessages.MemberNotFoundByUserId(userId));

        if (member.Role == ProjectRole.Owner
            && newRole != ProjectRole.Owner
            && _members.Count(m => m.Role == ProjectRole.Owner) <= 1)
        {
            throw new DomainException(DomainMessages.CannotDemoteLastOwner);
        }

        var oldRole = member.Role;
        member.UpdateRole(newRole);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberRoleUpdatedEvent(Id, userId, oldRole, newRole, DateTime.UtcNow));
    }

    public bool IsMember(Guid userId) => _members.Any(m => m.UserId == userId);

    public ProjectRole? GetMemberRole(Guid userId) =>
        _members.FirstOrDefault(m => m.UserId == userId)?.Role;

    #endregion

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainMessages.ProjectNameEmpty);

        if (name.Length > MaxNameLength)
            throw new DomainException(DomainMessages.ProjectNameTooLong);
    }

    private static void ValidateDescription(string? description)
    {
        if (description != null && description.Length > MaxDescriptionLength)
            throw new DomainException(DomainMessages.ProjectDescriptionTooLong);
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == default)
            throw new DomainException(DomainMessages.ProjectOwnerIdEmpty);
    }
}
