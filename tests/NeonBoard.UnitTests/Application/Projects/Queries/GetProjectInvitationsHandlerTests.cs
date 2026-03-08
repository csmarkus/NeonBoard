using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Queries.GetProjectInvitations;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Users;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Queries;

public class GetProjectInvitationsHandlerTests
{
    private readonly IProjectInvitationRepository _invitationRepository = Substitute.For<IProjectInvitationRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetProjectInvitationsHandler _handler;

    public GetProjectInvitationsHandlerTests()
    {
        _handler = new GetProjectInvitationsHandler(_invitationRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPendingInvitations()
    {
        var projectId = Guid.NewGuid();
        var inviterId = Guid.NewGuid();
        var invitation1 = ProjectInvitation.Create(
            projectId,
            "user1@example.com",
            ProjectRole.Editor,
            inviterId,
            DateTime.UtcNow.AddDays(7));
        var invitation2 = ProjectInvitation.Create(
            projectId,
            "user2@example.com",
            ProjectRole.Viewer,
            inviterId,
            DateTime.UtcNow.AddDays(7));

        var inviterUser = User.Create("auth0|inviter", "inviter@example.com", "Inviter User");

        _invitationRepository.GetPendingByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns([invitation1, invitation2]);
        _userRepository.GetByIdsAsync(
            Arg.Is<List<Guid>>(ids => ids.Contains(inviterId)),
            Arg.Any<CancellationToken>())
            .Returns([inviterUser]);

        var query = new GetProjectInvitationsQuery(projectId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(i =>
        {
            i.Status.Should().Be(InvitationStatus.Pending);
            i.Id.Should().NotBeEmpty();
        });
    }

    [Fact]
    public async Task Handle_WithNoInvitations_ShouldReturnEmptyList()
    {
        var projectId = Guid.NewGuid();

        _invitationRepository.GetPendingByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns([]);
        _userRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetProjectInvitationsQuery(projectId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
