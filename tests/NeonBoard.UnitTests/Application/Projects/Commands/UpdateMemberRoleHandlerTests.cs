using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.UpdateMemberRole;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class UpdateMemberRoleHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly UpdateMemberRoleHandler _handler;

    public UpdateMemberRoleHandlerTests()
    {
        _handler = new UpdateMemberRoleHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateRole()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        project.AddMember(memberId, ProjectRole.Viewer);

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new UpdateMemberRoleCommand(project.Id, memberId, ProjectRole.Editor);

        await _handler.Handle(command, CancellationToken.None);

        project.GetMemberRole(memberId).Should().Be(ProjectRole.Editor);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetWithMembersAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var command = new UpdateMemberRoleCommand(projectId, Guid.NewGuid(), ProjectRole.Editor);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsync()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        project.AddMember(memberId, ProjectRole.Viewer);

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new UpdateMemberRoleCommand(project.Id, memberId, ProjectRole.Editor);

        await _handler.Handle(command, CancellationToken.None);

        await _projectRepository.Received(1).UpdateAsync(project, Arg.Any<CancellationToken>());
    }
}
