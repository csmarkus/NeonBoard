using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.RemoveMember;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class RemoveMemberHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly RemoveMemberHandler _handler;

    public RemoveMemberHandlerTests()
    {
        _handler = new RemoveMemberHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveMember()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        project.AddMember(memberId, ProjectRole.Editor);

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new RemoveMemberCommand(project.Id, memberId);

        await _handler.Handle(command, CancellationToken.None);

        project.IsMember(memberId).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetWithMembersAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var command = new RemoveMemberCommand(projectId, Guid.NewGuid());

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

        var command = new RemoveMemberCommand(project.Id, memberId);

        await _handler.Handle(command, CancellationToken.None);

        await _projectRepository.Received(1).UpdateAsync(project, Arg.Any<CancellationToken>());
    }
}
