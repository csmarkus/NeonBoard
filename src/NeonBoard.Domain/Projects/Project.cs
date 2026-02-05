using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects.Events;

namespace NeonBoard.Domain.Projects;

public sealed class Project : Entity, IAggregateRoot
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 1000;

    public string Name { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public Guid OwnerId { get; private set; }

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
            Name = name,
            Description = description ?? string.Empty,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

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
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name cannot be empty.");

        if (name.Length > MaxNameLength)
            throw new DomainException($"Project name cannot exceed {MaxNameLength} characters.");
    }

    private static void ValidateDescription(string? description)
    {
        if (description != null && description.Length > MaxDescriptionLength)
            throw new DomainException($"Project description cannot exceed {MaxDescriptionLength} characters.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == default)
            throw new DomainException("Owner ID cannot be empty.");
    }
}
