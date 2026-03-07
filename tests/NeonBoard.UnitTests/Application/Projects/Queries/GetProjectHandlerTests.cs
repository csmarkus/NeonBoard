using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Queries.GetProject;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Queries;

public class GetProjectHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly GetProjectHandler _handler;

    public GetProjectHandlerTests()
    {
        _handler = new GetProjectHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnProjectDto()
    {
        var project = Project.Create("Test Project", "Description", Guid.NewGuid());
        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var query = new GetProjectQuery(project.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(project.Id);
        result.Name.Should().Be("Test Project");
        result.Description.Should().Be("Description");
        result.OwnerId.Should().Be(project.OwnerId);
        result.ShortId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var query = new GetProjectQuery(projectId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
