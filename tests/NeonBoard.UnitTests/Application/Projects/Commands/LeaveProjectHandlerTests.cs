using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.LeaveProject;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class LeaveProjectHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly LeaveProjectHandler _handler;

    public LeaveProjectHandlerTests()
    {
        _handler = new LeaveProjectHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveSelf()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        project.AddMember(memberId, ProjectRole.Editor);

        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new LeaveProjectCommand(project.Id, memberId);

        await _handler.Handle(command, CancellationToken.None);

        project.IsMember(memberId).Should().BeFalse();
        await _projectRepository.Received(1).UpdateAsync(project, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetWithMembersAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var command = new LeaveProjectCommand(projectId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
