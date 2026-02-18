using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class BoardEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    public BoardEndpointsTests(NeonBoardWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateBoard_InOwnedProject_ReturnsCreated()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/boards",
            new { Name = "Sprint Board" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var board = await response.Content.ReadFromJsonAsync<BoardDto>();
        board.Should().NotBeNull();
        board!.Name.Should().Be("Sprint Board");
        board.ProjectId.Should().Be(project.Id);
        board.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBoard_WithEmptyName_ReturnsBadRequest()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/boards",
            new { Name = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBoardsByProject_ReturnsBoardList()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board1 = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id, "Board A");
        var board2 = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id, "Board B");

        var response = await _client.GetAsync($"/api/projects/{project.Id}/boards");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var boards = await response.Content.ReadFromJsonAsync<List<BoardDto>>();
        boards.Should().NotBeNull();
        boards.Should().HaveCount(2);
        boards.Should().Contain(b => b.Name == "Board A");
        boards.Should().Contain(b => b.Name == "Board B");
    }

    [Fact]
    public async Task GetBoardDetails_ReturnsBoardWithColumnsAndCards()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id);
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, project.Id, board.Id, "To Do");
        await _factory.AddCardAsync<CardDto>(_client, project.Id, board.Id, column.Id, "Card 1");

        var response = await _client.GetAsync($"/api/projects/{project.Id}/boards/{board.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var details = await response.Content.ReadFromJsonAsync<BoardDetailsDto>();
        details.Should().NotBeNull();
        details!.Id.Should().Be(board.Id);
        details.Columns.Should().HaveCount(1);
        details.Columns[0].Name.Should().Be("To Do");
        details.Cards.Should().HaveCount(1);
        details.Cards[0].Title.Should().Be("Card 1");
    }

    [Fact]
    public async Task DeleteBoard_ReturnsNoContent()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id);

        var response = await _client.DeleteAsync($"/api/projects/{project.Id}/boards/{board.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
