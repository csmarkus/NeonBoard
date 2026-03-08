using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Projects.Entities;
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

    // --- Membership Tests ---

    [Fact]
    public void Create_ShouldAutoAddOwnerAsMember()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test", "Desc", ownerId);

        project.Members.Should().ContainSingle();
        project.Members[0].UserId.Should().Be(ownerId);
        project.Members[0].Role.Should().Be(ProjectRole.Owner);
    }

    [Fact]
    public void AddMember_WithValidData_ShouldAddMember()
    {
        var project = Project.Create("Test", "Desc", Guid.NewGuid());
        var userId = Guid.NewGuid();

        var member = project.AddMember(userId, ProjectRole.Editor);

        project.Members.Should().HaveCount(2);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(ProjectRole.Editor);
    }

    [Fact]
    public void AddMember_ShouldRaiseMemberAddedEvent()
    {
        var project = Project.Create("Test", "Desc", Guid.NewGuid());
        project.ClearDomainEvents();
        var userId = Guid.NewGuid();

        project.AddMember(userId, ProjectRole.Editor);

        project.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<MemberAddedToProjectEvent>();
    }

    [Fact]
    public void AddMember_WhenAlreadyMember_ShouldThrow()
    {
        var project = Project.Create("Test", "Desc", Guid.NewGuid());
        var userId = Guid.NewGuid();
        project.AddMember(userId, ProjectRole.Editor);

        var act = () => project.AddMember(userId, ProjectRole.Viewer);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.MemberAlreadyExists);
    }

    [Fact]
    public void RemoveMember_ShouldRemoveMember()
    {
        var project = Project.Create("Test", "Desc", Guid.NewGuid());
        var userId = Guid.NewGuid();
        project.AddMember(userId, ProjectRole.Editor);

        project.RemoveMember(userId);

        project.Members.Should().ContainSingle();
    }

    [Fact]
    public void RemoveMember_LastOwner_ShouldThrow()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test", "Desc", ownerId);

        var act = () => project.RemoveMember(ownerId);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CannotRemoveLastOwner);
    }

    [Fact]
    public void RemoveMember_NotLastOwner_ShouldSucceed()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test", "Desc", ownerId);
        var secondOwner = Guid.NewGuid();
        project.AddMember(secondOwner, ProjectRole.Owner);

        project.RemoveMember(ownerId);

        project.Members.Should().ContainSingle()
            .Which.UserId.Should().Be(secondOwner);
    }

    [Fact]
    public void UpdateMemberRole_ShouldChangeRole()
    {
        var project = Project.Create("Test", "Desc", Guid.NewGuid());
        var userId = Guid.NewGuid();
        project.AddMember(userId, ProjectRole.Viewer);

        project.UpdateMemberRole(userId, ProjectRole.Editor);

        project.Members.First(m => m.UserId == userId).Role.Should().Be(ProjectRole.Editor);
    }

    [Fact]
    public void UpdateMemberRole_DemoteLastOwner_ShouldThrow()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test", "Desc", ownerId);

        var act = () => project.UpdateMemberRole(ownerId, ProjectRole.Editor);

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.CannotDemoteLastOwner);
    }

    [Fact]
    public void IsMember_ShouldReturnCorrectResult()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test", "Desc", ownerId);

        project.IsMember(ownerId).Should().BeTrue();
        project.IsMember(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void GetMemberRole_ShouldReturnCorrectRole()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test", "Desc", ownerId);

        project.GetMemberRole(ownerId).Should().Be(ProjectRole.Owner);
        project.GetMemberRole(Guid.NewGuid()).Should().BeNull();
    }
}
