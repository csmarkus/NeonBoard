using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Projects.Events;

namespace NeonBoard.UnitTests.Domain.Projects;

public class ProjectTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProject()
    {
        var ownerId = Guid.NewGuid();

        var project = Project.Create("My Project", "A description", ownerId);

        project.Should().NotBeNull();
        project.Id.Should().NotBeEmpty();
        project.Name.Should().Be("My Project");
        project.Description.Should().Be("A description");
        project.OwnerId.Should().Be(ownerId);
        project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldRaiseProjectCreatedEvent()
    {
        var ownerId = Guid.NewGuid();

        var project = Project.Create("My Project", "Desc", ownerId);

        var domainEvent = project.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProjectCreatedEvent>().Subject;

        domainEvent.ProjectId.Should().Be(project.Id);
        domainEvent.Name.Should().Be("My Project");
        domainEvent.OwnerId.Should().Be(ownerId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrow(string? name)
    {
        var act = () => Project.Create(name!, "Desc", Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ProjectNameEmpty);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrow()
    {
        var longName = new string('a', 101);

        var act = () => Project.Create(longName, "Desc", Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ProjectNameTooLong);
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldThrow()
    {
        var longDesc = new string('a', 1001);

        var act = () => Project.Create("Project", longDesc, Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ProjectDescriptionTooLong);
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldThrow()
    {
        var act = () => Project.Create("Project", "Desc", Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ProjectOwnerIdEmpty);
    }

    [Fact]
    public void Update_ShouldChangeNameAndDescription()
    {
        var project = Project.Create("Old Name", "Old Desc", Guid.NewGuid());
        var originalCreatedAt = project.CreatedAt;

        project.Update("New Name", "New Desc");

        project.Name.Should().Be("New Name");
        project.Description.Should().Be("New Desc");
        project.CreatedAt.Should().Be(originalCreatedAt);
        project.UpdatedAt.Should().BeOnOrAfter(originalCreatedAt);
    }

    [Fact]
    public void Update_ShouldRaiseProjectUpdatedEvent()
    {
        var project = Project.Create("Original", "Desc", Guid.NewGuid());
        project.ClearDomainEvents();

        project.Update("Updated Name", "Updated Desc");

        var domainEvent = project.GetDomainEvents()
            .OfType<ProjectUpdatedEvent>()
            .SingleOrDefault();

        domainEvent.Should().NotBeNull();
        domainEvent!.ProjectId.Should().Be(project.Id);
        domainEvent.Name.Should().Be("Updated Name");
        domainEvent.Description.Should().Be("Updated Desc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ShouldThrow(string? name)
    {
        var project = Project.Create("Project", "Desc", Guid.NewGuid());

        var act = () => project.Update(name!, "Desc");

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.ProjectNameEmpty);
    }

    [Fact]
    public void Update_WithNullDescription_ShouldDefaultToEmpty()
    {
        var project = Project.Create("Project", "Desc", Guid.NewGuid());

        project.Update("Project", null!);

        project.Description.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldGenerateShortId()
    {
        var project = Project.Create("Test Project", "Description", Guid.NewGuid());

        project.ShortId.Should().NotBeNullOrWhiteSpace();
        project.ShortId.Should().HaveLength(7);
    }

    [Fact]
    public void Create_ShortId_ShouldOnlyContainUnambiguousCharacters()
    {
        var ambiguousChars = new[] { '0', '1', 'O', 'I', 'l' };

        for (int i = 0; i < 20; i++)
        {
            var project = Project.Create($"Test Project {i}", "Description", Guid.NewGuid());
            project.ShortId.Should().NotContainAny(ambiguousChars.Select(c => c.ToString()).ToArray());
        }
    }
}
