using FluentAssertions;
using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Projects.Events;

namespace NeonBoard.UnitTests.Domain.Projects;

public class ProjectInvitationTests
{
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _inviterId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateInvitation()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));

        invitation.ProjectId.Should().Be(_projectId);
        invitation.Email.Should().Be("user@example.com");
        invitation.Role.Should().Be(ProjectRole.Editor);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.Token.Should().NotBeNullOrEmpty();
        invitation.InvitedByUserId.Should().Be(_inviterId);
    }

    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "  USER@Example.COM  ", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));

        invitation.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_ShouldRaiseInvitationCreatedEvent()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));

        invitation.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<InvitationCreatedEvent>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_ShouldThrow(string? email)
    {
        var act = () => ProjectInvitation.Create(
            _projectId, email!, ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.InvitationEmailEmpty);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrow()
    {
        var act = () => ProjectInvitation.Create(
            _projectId, "not-an-email", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.InvitationEmailInvalid);
    }

    [Fact]
    public void Accept_WhenPending_ShouldAccept()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));
        invitation.ClearDomainEvents();
        var accepterId = Guid.NewGuid();

        invitation.Accept(accepterId);

        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedByUserId.Should().Be(accepterId);
    }

    [Fact]
    public void Accept_ShouldRaiseInvitationAcceptedEvent()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));
        invitation.ClearDomainEvents();

        invitation.Accept(Guid.NewGuid());

        invitation.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<InvitationAcceptedEvent>();
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_ShouldThrow()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));
        invitation.Accept(Guid.NewGuid());

        var act = () => invitation.Accept(Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.InvitationAlreadyAccepted);
    }

    [Fact]
    public void Accept_WhenRevoked_ShouldThrow()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));
        invitation.Revoke();

        var act = () => invitation.Accept(Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.InvitationAlreadyRevoked);
    }

    [Fact]
    public void Revoke_WhenPending_ShouldRevoke()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));

        invitation.Revoke();

        invitation.Status.Should().Be(InvitationStatus.Revoked);
    }

    [Fact]
    public void Revoke_WhenNotPending_ShouldThrow()
    {
        var invitation = ProjectInvitation.Create(
            _projectId, "user@example.com", ProjectRole.Editor,
            _inviterId, DateTime.UtcNow.AddDays(7));
        invitation.Accept(Guid.NewGuid());

        var act = () => invitation.Revoke();

        act.Should().Throw<DomainException>()
            .WithMessage(DomainMessages.InvitationNotPending);
    }
}
