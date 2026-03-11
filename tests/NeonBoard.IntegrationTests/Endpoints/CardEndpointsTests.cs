using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class CardEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    public CardEndpointsTests(NeonBoardWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid ProjectId, Guid BoardId, Guid ColumnId)> CreateProjectBoardColumnAsync()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id);
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, project.Id, board.Id, "To Do");
        return (project.Id, board.Id, column.Id);
    }

    [Fact]
    public async Task AddCard_ReturnsCreatedWithCardDto()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards",
            new { ColumnId = columnId, Title = "New Card", Description = "Card description" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var card = await response.Content.ReadFromJsonAsync<CardDto>();
        card.Should().NotBeNull();
        card!.Title.Should().Be("New Card");
        card.Description.Should().Be("Card description");
        card.ColumnId.Should().Be(columnId);
        card.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateCard_ReturnsNoContent()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId);

        var response = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}",
            new { Title = "Updated Title", Description = "Updated description" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Cards.Should().Contain(c => c.Title == "Updated Title");
    }

    [Fact]
    public async Task MoveCard_ReturnsNoContent()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId);

        var targetColumn = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId, "Done");

        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}/move");
        request.Content = JsonContent.Create(new { TargetColumnId = targetColumn.Id, NewPosition = "a0" });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Cards.Should().Contain(c => c.Id == card.Id && c.ColumnId == targetColumn.Id);
    }

    [Fact]
    public async Task DeleteCard_ReturnsNoContent()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId);

        var response = await _client.DeleteAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Cards.Should().NotContain(c => c.Id == card.Id);
    }
}
