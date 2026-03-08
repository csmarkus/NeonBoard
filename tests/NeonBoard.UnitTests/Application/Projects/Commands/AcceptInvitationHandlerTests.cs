using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.AcceptInvitation;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class AcceptInvitationHandlerTests
{
    private readonly IProjectInvitationRepository _invitationRepository = Substitute.For<IProjectInvitationRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly AcceptInvitationHandler _handler;

    public AcceptInvitationHandlerTests()
    {
        _handler = new AcceptInvitationHandler(_invitationRepository, _projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldAcceptAndAddMember()
    {
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        var invitation = ProjectInvitation.Create(
            project.Id,
            "newuser@example.com",
            ProjectRole.Editor,
            ownerId,
            DateTime.UtcNow.AddDays(7));

        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new AcceptInvitationCommand(invitation.Token, userId);

        await _handler.Handle(command, CancellationToken.None);

        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedByUserId.Should().Be(userId);
        project.IsMember(userId).Should().BeTrue();

        await _invitationRepository.Received(1).UpdateAsync(invitation, Arg.Any<CancellationToken>());
        await _projectRepository.Received(1).UpdateAsync(project, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInvitationNotFound_ShouldThrowNotFoundException()
    {
        _invitationRepository.GetByTokenAsync("invalid-token", Arg.Any<CancellationToken>())
            .Returns((ProjectInvitation?)null);

        var command = new AcceptInvitationCommand("invalid-token", Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyMember_ShouldNotAddDuplicateMember()
    {
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);

        // Pre-add the user as a member
        project.AddMember(userId, ProjectRole.Viewer);

        var invitation = ProjectInvitation.Create(
            project.Id,
            "existingmember@example.com",
            ProjectRole.Editor,
            ownerId,
            DateTime.UtcNow.AddDays(7));

        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new AcceptInvitationCommand(invitation.Token, userId);

        await _handler.Handle(command, CancellationToken.None);

        // The invitation should be accepted
        invitation.Status.Should().Be(InvitationStatus.Accepted);

        // The user should still only appear once in the members list
        project.Members.Count(m => m.UserId == userId).Should().Be(1);
    }
}
