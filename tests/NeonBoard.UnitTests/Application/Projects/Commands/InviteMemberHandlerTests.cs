using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.InviteMember;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Users;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class InviteMemberHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IProjectInvitationRepository _invitationRepository = Substitute.For<IProjectInvitationRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly InviteMemberHandler _handler;

    public InviteMemberHandlerTests()
    {
        _handler = new InviteMemberHandler(
            _projectRepository,
            _invitationRepository,
            _userRepository,
            _emailService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnInvitationDto()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        var inviterId = ownerId;
        var inviter = User.Create("auth0|inviter", "inviter@example.com", "Inviter User");

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _userRepository.GetByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _invitationRepository.HasPendingInvitationAsync(project.Id, "newuser@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetByIdAsync(inviterId, Arg.Any<CancellationToken>())
            .Returns(inviter);

        var command = new InviteMemberCommand(project.Id, "newuser@example.com", ProjectRole.Editor, inviterId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Email.Should().Be("newuser@example.com");
        result.Role.Should().Be(ProjectRole.Editor);
        result.Status.Should().Be(InvitationStatus.Pending);
        result.InvitedByName.Should().Be("Inviter User");
    }

    [Fact]
    public async Task Handle_ShouldSendInvitationEmail()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        var inviter = User.Create("auth0|inviter", "inviter@example.com", "Inviter User");

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _userRepository.GetByEmailAsync("newuser@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _invitationRepository.HasPendingInvitationAsync(project.Id, "newuser@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetByIdAsync(ownerId, Arg.Any<CancellationToken>())
            .Returns(inviter);

        var command = new InviteMemberCommand(project.Id, "newuser@example.com", ProjectRole.Editor, ownerId);

        await _handler.Handle(command, CancellationToken.None);

        await _emailService.Received(1).SendInvitationEmailAsync(
            "newuser@example.com",
            "Test Project",
            "Inviter User",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetWithMembersAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var command = new InviteMemberCommand(projectId, "user@example.com", ProjectRole.Editor, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyMember_ShouldThrowConflictException()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        var existingUser = User.Create("auth0|existing", "existing@example.com", "Existing User");

        // Add the existing user as a member
        project.AddMember(existingUser.Id, ProjectRole.Editor);

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _userRepository.GetByEmailAsync("existing@example.com", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var command = new InviteMemberCommand(project.Id, "existing@example.com", ProjectRole.Editor, ownerId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already a member*");
    }

    [Fact]
    public async Task Handle_WhenPendingInvitationExists_ShouldThrowConflictException()
    {
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _userRepository.GetByEmailAsync("pending@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _invitationRepository.HasPendingInvitationAsync(project.Id, "pending@example.com", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new InviteMemberCommand(project.Id, "pending@example.com", ProjectRole.Editor, ownerId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*pending invitation*");
    }
}
