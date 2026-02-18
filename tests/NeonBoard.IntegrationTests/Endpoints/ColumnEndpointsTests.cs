using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class ColumnEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    public ColumnEndpointsTests(NeonBoardWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid ProjectId, Guid BoardId)> CreateProjectAndBoardAsync()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id);
        return (project.Id, board.Id);
    }

    [Fact]
    public async Task AddColumn_ReturnsCreatedWithColumnDto()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/columns",
            new { Name = "In Progress" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var column = await response.Content.ReadFromJsonAsync<ColumnDto>();
        column.Should().NotBeNull();
        column!.Name.Should().Be("In Progress");
        column.BoardId.Should().Be(boardId);
        column.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RenameColumn_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId, "Old Name");

        var response = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/columns/{column.Id}",
            new { NewName = "New Name" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify via board details
        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Columns.Should().Contain(c => c.Name == "New Name");
    }

    [Fact]
    public async Task ReorderColumns_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var col1 = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId, "First");
        var col2 = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId, "Second");

        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/projects/{projectId}/boards/{boardId}/columns/reorder");
        request.Content = JsonContent.Create(new { ColumnIds = new[] { col2.Id, col1.Id } });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Columns.Should().HaveCount(2);
        details.Columns[0].Name.Should().Be("Second");
        details.Columns[1].Name.Should().Be("First");
    }

    [Fact]
    public async Task DeleteColumn_WhenEmpty_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId, "To Delete");

        var response = await _client.DeleteAsync(
            $"/api/projects/{projectId}/boards/{boardId}/columns/{column.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteColumn_WithCards_ReturnsBadRequest()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId, "Has Cards");
        await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, column.Id, "Blocking Card");

        var response = await _client.DeleteAsync(
            $"/api/projects/{projectId}/boards/{boardId}/columns/{column.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
