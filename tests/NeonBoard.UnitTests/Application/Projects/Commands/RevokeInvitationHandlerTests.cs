using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.RevokeInvitation;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class RevokeInvitationHandlerTests
{
    private readonly IProjectInvitationRepository _invitationRepository = Substitute.For<IProjectInvitationRepository>();
    private readonly RevokeInvitationHandler _handler;

    public RevokeInvitationHandlerTests()
    {
        _handler = new RevokeInvitationHandler(_invitationRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRevokeInvitation()
    {
        var projectId = Guid.NewGuid();
        var inviterId = Guid.NewGuid();
        var invitation = ProjectInvitation.Create(
            projectId,
            "user@example.com",
            ProjectRole.Editor,
            inviterId,
            DateTime.UtcNow.AddDays(7));

        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var command = new RevokeInvitationCommand(projectId, invitation.Id);

        await _handler.Handle(command, CancellationToken.None);

        invitation.Status.Should().Be(InvitationStatus.Revoked);
        await _invitationRepository.Received(1).UpdateAsync(invitation, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInvitationNotFound_ShouldThrowNotFoundException()
    {
        var invitationId = Guid.NewGuid();
        _invitationRepository.GetByIdAsync(invitationId, Arg.Any<CancellationToken>())
            .Returns((ProjectInvitation?)null);

        var command = new RevokeInvitationCommand(Guid.NewGuid(), invitationId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenInvitationBelongsToDifferentProject_ShouldThrowNotFoundException()
    {
        var actualProjectId = Guid.NewGuid();
        var differentProjectId = Guid.NewGuid();
        var inviterId = Guid.NewGuid();
        var invitation = ProjectInvitation.Create(
            actualProjectId,
            "user@example.com",
            ProjectRole.Editor,
            inviterId,
            DateTime.UtcNow.AddDays(7));

        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var command = new RevokeInvitationCommand(differentProjectId, invitation.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
