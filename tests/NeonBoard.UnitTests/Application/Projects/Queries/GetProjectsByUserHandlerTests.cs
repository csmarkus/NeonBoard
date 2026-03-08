using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Queries.GetProjectsByUser;
using NeonBoard.Domain.Projects;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Queries;

public class GetProjectsByUserHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly GetProjectsByUserHandler _handler;

    public GetProjectsByUserHandlerTests()
    {
        _handler = new GetProjectsByUserHandler(_projectRepository);
    }

    [Fact]
    public async Task Handle_WithProjects_ShouldReturnProjectDtoList()
    {
        var userId = Guid.NewGuid();
        var project1 = Project.Create("Project 1", "Desc 1", userId);
        var project2 = Project.Create("Project 2", "Desc 2", userId);

        _projectRepository.GetProjectsByMemberUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns([project1, project2]);

        var query = new GetProjectsByUserQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Project 1");
        result[0].OwnerId.Should().Be(userId);
        result[1].Name.Should().Be("Project 2");
    }

    [Fact]
    public async Task Handle_WithNoProjects_ShouldReturnEmptyList()
    {
        var userId = Guid.NewGuid();
        _projectRepository.GetProjectsByMemberUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetProjectsByUserQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
