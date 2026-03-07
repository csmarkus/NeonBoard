using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.DeleteProject;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class DeleteProjectHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly DeleteProjectHandler _handler;

    public DeleteProjectHandlerTests()
    {
        _handler = new DeleteProjectHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUnit()
    {
        var project = Project.Create("Test Project", "Description", Guid.NewGuid());
        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new DeleteProjectCommand(project.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallDeleteAsync()
    {
        var project = Project.Create("Test Project", "Description", Guid.NewGuid());
        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var command = new DeleteProjectCommand(project.Id);

        await _handler.Handle(command, CancellationToken.None);

        await _projectRepository.Received(1).DeleteAsync(
            Arg.Is<Project>(p => p.Id == project.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var command = new DeleteProjectCommand(projectId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
