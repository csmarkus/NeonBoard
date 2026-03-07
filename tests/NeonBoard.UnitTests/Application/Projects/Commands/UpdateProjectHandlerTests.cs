using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.UpdateProject;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class UpdateProjectHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly UpdateProjectHandler _handler;

    public UpdateProjectHandlerTests()
    {
        _handler = new UpdateProjectHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUpdatedProjectDto()
    {
        var project = Project.Create("Old Name", "Old Description", Guid.NewGuid());
        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new UpdateProjectCommand(project.Id, "New Name", "New Description");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(project.Id);
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Description");
        result.OwnerId.Should().Be(project.OwnerId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallUpdateAsync()
    {
        var project = Project.Create("Old Name", "Old Description", Guid.NewGuid());
        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new UpdateProjectCommand(project.Id, "New Name", "New Description");

        await _handler.Handle(command, CancellationToken.None);

        await _projectRepository.Received(1).UpdateAsync(
            Arg.Is<Project>(p => p.Id == project.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var command = new UpdateProjectCommand(projectId, "Name", "Description");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
