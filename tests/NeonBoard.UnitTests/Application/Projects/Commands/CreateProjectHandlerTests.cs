using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Commands.CreateProject;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Commands;

public class CreateProjectHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly CreateProjectHandler _handler;

    public CreateProjectHandlerTests()
    {
        _handler = new CreateProjectHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnProjectDto()
    {
        var ownerId = Guid.NewGuid();
        var command = new CreateProjectCommand("Test Project", "A description", ownerId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Project");
        result.Description.Should().Be("A description");
        result.OwnerId.Should().Be(ownerId);
        result.ShortId.Should().NotBeNullOrEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        var ownerId = Guid.NewGuid();
        var command = new CreateProjectCommand("Test Project", "A description", ownerId);

        await _handler.Handle(command, CancellationToken.None);

        await _projectRepository.Received(1).AddAsync(
            Arg.Is<Project>(p => p.Name == "Test Project" && p.OwnerId == ownerId),
            Arg.Any<CancellationToken>());
    }
}
