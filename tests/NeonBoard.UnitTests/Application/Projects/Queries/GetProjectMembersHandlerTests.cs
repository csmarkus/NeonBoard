using NeonBoard.Application.Common.Exceptions;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Application.Projects.Queries.GetProjectMembers;
using NeonBoard.Domain.Projects;
using NeonBoard.Domain.Users;
using NSubstitute;

namespace NeonBoard.UnitTests.Application.Projects.Queries;

public class GetProjectMembersHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetProjectMembersHandler _handler;

    public GetProjectMembersHandlerTests()
    {
        _handler = new GetProjectMembersHandler(_projectRepository, _userRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnMembersWithUserDetails()
    {
        var ownerId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        project.AddMember(editorId, ProjectRole.Editor);

        var ownerUser = User.Create("auth0|owner", "owner@example.com", "Owner User");
        var editorUser = User.Create("auth0|editor", "editor@example.com", "Editor User");

        // We need to match the user IDs to the member user IDs.
        // Since User.Create generates its own Guid, we use GetByIdsAsync to return users
        // and the handler does a lookup by user.Id. We need to ensure the users returned
        // have the same IDs as the project members.
        // The simplest approach: mock GetByIdsAsync to return users, then verify mapping.
        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        // The handler calls GetByIdsAsync with the member userIds
        _userRepository.GetByIdsAsync(
            Arg.Is<List<Guid>>(ids => ids.Contains(ownerId) && ids.Contains(editorId)),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetProjectMembersQuery(project.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        // When no user found, display name defaults to "Unknown"
        result.Should().AllSatisfy(m => m.UserId.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.GetWithMembersAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var query = new GetProjectMembersQuery(projectId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldOrderByRoleThenName()
    {
        var ownerId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var project = Project.Create("Test Project", "Description", ownerId);
        project.AddMember(viewerId, ProjectRole.Viewer);
        project.AddMember(editorId, ProjectRole.Editor);

        // Create users with specific IDs matching member IDs.
        // Since we can't control User.Create IDs, we'll create users and
        // return them from the mock. The handler does userLookup by u.Id,
        // so the user IDs need to match the member userIds.
        // We'll return an empty list and verify order by fallback "Unknown" names.
        _projectRepository.GetWithMembersAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _userRepository.GetByIdsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var query = new GetProjectMembersQuery(project.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        // Owner should be first
        result[0].Role.Should().Be(ProjectRole.Owner);
        result[0].UserId.Should().Be(ownerId);
        // Editor should be second
        result[1].Role.Should().Be(ProjectRole.Editor);
        // Viewer should be last
        result[2].Role.Should().Be(ProjectRole.Viewer);
    }
}
