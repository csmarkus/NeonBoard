using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class ProjectEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    public ProjectEndpointsTests(NeonBoardWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProject_WithValidData_ReturnsCreatedWithProjectDto()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new
        {
            Name = "My Project",
            Description = "My project description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Name.Should().Be("My Project");
        project.Description.Should().Be("My project description");
        project.OwnerId.Should().Be(_factory.TestUserId);
        project.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProject_WithEmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new
        {
            Name = "",
            Description = "Some description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProjectsByCurrentUser_ReturnsOwnedProjects()
    {
        var uniqueName = $"List Project {Guid.NewGuid():N}";
        await _factory.CreateProjectAsync<ProjectDto>(_client, uniqueName);

        var response = await _client.GetAsync("/api/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        projects.Should().NotBeNull();
        projects.Should().Contain(p => p.Name == uniqueName);
    }

    [Fact]
    public async Task GetProject_WhenOwned_ReturnsOk()
    {
        var created = await _factory.CreateProjectAsync<ProjectDto>(_client);

        var response = await _client.GetAsync($"/api/projects/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Id.Should().Be(created.Id);
        project.Name.Should().Be(created.Name);
    }

    [Fact]
    public async Task GetProject_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProject_WithValidData_ReturnsOkWithUpdatedDto()
    {
        var created = await _factory.CreateProjectAsync<ProjectDto>(_client);

        var response = await _client.PutAsJsonAsync($"/api/projects/{created.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated description"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<ProjectDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("Updated description");
        updated.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task DeleteProject_WhenOwned_ReturnsNoContent()
    {
        var created = await _factory.CreateProjectAsync<ProjectDto>(_client);

        var response = await _client.DeleteAsync($"/api/projects/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/projects/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
