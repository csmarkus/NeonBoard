using System.Text;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class ArchivedCardsEndpointTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    public ArchivedCardsEndpointTests(NeonBoardWebApplicationFactory factory)
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
    public async Task GetArchivedCards_ReturnsEmptyList_WhenNoArchivedCards()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId);

        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/archived");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cards = await response.Content.ReadFromJsonAsync<List<CardDto>>();
        cards.Should().NotBeNull();
        cards.Should().BeEmpty();
    }

    [Fact]
    public async Task GetArchivedCards_ReturnsArchivedCards()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId);

        var archiveRequest = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}/archive");
        archiveRequest.Content = new StringContent("", Encoding.UTF8, "application/json");
        var archiveResponse = await _client.SendAsync(archiveRequest);
        archiveResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/archived");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cards = await response.Content.ReadFromJsonAsync<List<CardDto>>();
        cards.Should().NotBeNull();
        cards.Should().HaveCount(1);
        cards![0].Id.Should().Be(card.Id);
        cards[0].ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetArchivedCards_DoesNotReturnActiveCards()
    {
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        var card1 = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId, "Card 1");
        var card2 = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId, "Card 2");

        var archiveRequest = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card1.Id}/archive");
        archiveRequest.Content = new StringContent("", Encoding.UTF8, "application/json");
        var archiveResponse = await _client.SendAsync(archiveRequest);
        archiveResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/archived");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cards = await response.Content.ReadFromJsonAsync<List<CardDto>>();
        cards.Should().NotBeNull();
        cards.Should().HaveCount(1);
        cards![0].Id.Should().Be(card1.Id);
        cards.Should().NotContain(c => c.Id == card2.Id);
    }
}
