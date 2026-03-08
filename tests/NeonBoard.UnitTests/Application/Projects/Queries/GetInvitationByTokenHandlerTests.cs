using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Queries.GetInvitationByToken;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Users;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Queries;

public class GetInvitationByTokenHandlerTests
{
    private readonly IProjectInvitationRepository _invitationRepository = Substitute.For<IProjectInvitationRepository>();
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetInvitationByTokenHandler _handler;

    public GetInvitationByTokenHandlerTests()
    {
        _handler = new GetInvitationByTokenHandler(
            _invitationRepository,
            _projectRepository,
            _userRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvitationDetails()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        var inviter = User.Create("auth0|inviter", "inviter@example.com", "Inviter User");
        var invitation = ProjectInvitation.Create(
            project.Id,
            "user@example.com",
            ProjectRole.Editor,
            inviter.Id,
            DateTime.UtcNow.AddDays(7));

        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>())
            .Returns(invitation);
        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _userRepository.GetByIdAsync(inviter.Id, Arg.Any<CancellationToken>())
            .Returns(inviter);

        var query = new GetInvitationByTokenQuery(invitation.Token);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(invitation.Id);
        result.ProjectName.Should().Be("Test Project");
        result.InviterName.Should().Be("Inviter User");
        result.Role.Should().Be(ProjectRole.Editor);
        result.Status.Should().Be(InvitationStatus.Pending);
        result.IsExpired.Should().BeFalse();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WhenInvitationNotFound_ShouldThrowNotFoundException()
    {
        _invitationRepository.GetByTokenAsync("invalid-token", Arg.Any<CancellationToken>())
            .Returns((ProjectInvitation?)null);

        var query = new GetInvitationByTokenQuery("invalid-token");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
