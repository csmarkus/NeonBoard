using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class LabelEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    public LabelEndpointsTests(NeonBoardWebApplicationFactory factory)
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
    public async Task AddLabel_ReturnsCreatedWithLabelDto()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/labels",
            new { Name = "Bug", Color = "red" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var label = await response.Content.ReadFromJsonAsync<LabelDto>();
        label.Should().NotBeNull();
        label!.Name.Should().Be("Bug");
        label.Color.Should().Be("red");
        label.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateLabel_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var label = await _factory.AddLabelAsync<LabelDto>(_client, projectId, boardId, "Old", "red");

        var response = await _client.PutAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/labels/{label.Id}",
            new { Name = "Feature", Color = "blue" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Labels.Should().Contain(l => l.Name == "Feature" && l.Color == "blue");
    }

    [Fact]
    public async Task RemoveLabel_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var label = await _factory.AddLabelAsync<LabelDto>(_client, projectId, boardId, "ToRemove", "grey");

        var response = await _client.DeleteAsync(
            $"/api/projects/{projectId}/boards/{boardId}/labels/{label.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        details!.Labels.Should().NotContain(l => l.Id == label.Id);
    }

    [Fact]
    public async Task AddCardLabel_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId);
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, column.Id);
        var label = await _factory.AddLabelAsync<LabelDto>(_client, projectId, boardId, "Priority", "orange");

        var response = await _client.PutAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}/labels/{label.Id}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        var updatedCard = details!.Cards.First(c => c.Id == card.Id);
        updatedCard.Labels.Should().Contain(l => l.Id == label.Id);
    }

    [Fact]
    public async Task RemoveCardLabel_ReturnsNoContent()
    {
        var (projectId, boardId) = await CreateProjectAndBoardAsync();
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, projectId, boardId);
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, column.Id);
        var label = await _factory.AddLabelAsync<LabelDto>(_client, projectId, boardId, "ToUnassign", "cyan");

        await _client.PutAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}/labels/{label.Id}",
            null);

        var response = await _client.DeleteAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}/labels/{label.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var details = await _client.GetFromJsonAsync<BoardDetailsDto>(
            $"/api/projects/{projectId}/boards/{boardId}");
        var updatedCard = details!.Cards.First(c => c.Id == card.Id);
        updatedCard.Labels.Should().NotContain(l => l.Id == label.Id);
    }
}
