using System.Text.Json;
using NeonBoard.Application.Boards.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Projects.DTOs;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class ActivityEndpointsTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NeonBoardWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ActivityEndpointsTests(NeonBoardWebApplicationFactory factory)
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

    private async Task<(Guid ProjectId, Guid BoardId, Guid ColumnId)> CreateProjectBoardColumnAsync()
    {
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id);
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, project.Id, board.Id, "To Do");
        return (project.Id, board.Id, column.Id);
    }

    [Fact]
    public async Task GetBoardActivity_AfterCreatingBoard_ShouldReturnBoardCreatedEntry()
    {
        // Arrange
        var (projectId, boardId) = await CreateProjectAndBoardAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/boards/{boardId}/activity?pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var feed = await response.Content.ReadFromJsonAsync<ActivityFeedResponse>(JsonOptions);
        feed.Should().NotBeNull();
        feed!.Entries.Should().NotBeEmpty();

        var boardCreatedEntry = feed.Entries.Should()
            .Contain(e => e.EntityType == "Board" && e.ActionType == "Created")
            .Which;

        boardCreatedEntry.BoardId.Should().Be(boardId);
        boardCreatedEntry.EntityId.Should().Be(boardId);
        boardCreatedEntry.UserId.Should().Be(_factory.TestUserId);
        boardCreatedEntry.UserName.Should().Be(TestAuthHandler.TestName);
        boardCreatedEntry.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task GetBoardActivity_AfterCardOperations_ShouldReturnMultipleEntries()
    {
        // Arrange
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId, "My Card");

        // Act
        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/boards/{boardId}/activity?pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var feed = await response.Content.ReadFromJsonAsync<ActivityFeedResponse>(JsonOptions);
        feed.Should().NotBeNull();

        // Should have at least 3 entries: board created + column added + card created
        feed!.Entries.Should().HaveCountGreaterThanOrEqualTo(3);

        feed.Entries.Should().Contain(e => e.EntityType == "Board" && e.ActionType == "Created");
        feed.Entries.Should().Contain(e => e.EntityType == "Column" && e.ActionType == "Created");
        feed.Entries.Should().Contain(e => e.EntityType == "Card" && e.ActionType == "Created");
    }

    [Fact]
    public async Task GetCardActivity_ShouldReturnOnlyCardSpecificEntries()
    {
        // Arrange
        var (projectId, boardId, columnId) = await CreateProjectBoardColumnAsync();
        var card = await _factory.AddCardAsync<CardDto>(_client, projectId, boardId, columnId, "Specific Card");

        // Act
        var response = await _client.GetAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards/{card.Id}/activity?pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var feed = await response.Content.ReadFromJsonAsync<ActivityFeedResponse>(JsonOptions);
        feed.Should().NotBeNull();

        // Should only return card-specific entries, not board/column events
        feed!.Entries.Should().NotBeEmpty();
        feed.Entries.Should().OnlyContain(e => e.EntityType == "Card");
        feed.Entries.Should().OnlyContain(e => e.EntityId == card.Id);

        var cardCreatedEntry = feed.Entries.Should()
            .Contain(e => e.ActionType == "Created")
            .Which;

        cardCreatedEntry.EntityId.Should().Be(card.Id);
    }

    [Fact]
    public async Task GetBoardActivity_Pagination_ShouldReturnNextCursor()
    {
        // Arrange - create enough activity entries to paginate
        var project = await _factory.CreateProjectAsync<ProjectDto>(_client);
        var board = await _factory.CreateBoardAsync<BoardDto>(_client, project.Id); // 1 entry: board created
        var column = await _factory.AddColumnAsync<ColumnDto>(_client, project.Id, board.Id, "Col 1"); // 2 entries

        // Create multiple cards to generate more activity entries
        await _factory.AddCardAsync<CardDto>(_client, project.Id, board.Id, column.Id, "Card 1"); // 3 entries
        await _factory.AddCardAsync<CardDto>(_client, project.Id, board.Id, column.Id, "Card 2"); // 4 entries
        await _factory.AddCardAsync<CardDto>(_client, project.Id, board.Id, column.Id, "Card 3"); // 5 entries

        // Act - request with small page size
        var firstPageResponse = await _client.GetAsync(
            $"/api/projects/{project.Id}/boards/{board.Id}/activity?pageSize=2");

        // Assert first page
        firstPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<ActivityFeedResponse>(JsonOptions);
        firstPage.Should().NotBeNull();
        firstPage!.Entries.Should().HaveCount(2);
        firstPage.NextCursor.Should().NotBeNull("there are more entries beyond the first page");

        // Act - request second page using cursor
        var cursorValue = firstPage.NextCursor!.Value.ToString("O");
        var secondPageResponse = await _client.GetAsync(
            $"/api/projects/{project.Id}/boards/{board.Id}/activity?pageSize=2&cursor={cursorValue}");

        // Assert second page
        secondPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<ActivityFeedResponse>(JsonOptions);
        secondPage.Should().NotBeNull();
        secondPage!.Entries.Should().HaveCount(2);

        // Entries on the second page should be older (occurred earlier) than those on the first page
        var oldestOnFirstPage = firstPage.Entries.Min(e => e.OccurredAt);
        var newestOnSecondPage = secondPage.Entries.Max(e => e.OccurredAt);
        newestOnSecondPage.Should().BeBefore(oldestOnFirstPage.AddSeconds(1),
            "second page entries should be older than first page entries");

        // No overlap in entry IDs between pages
        var firstPageIds = firstPage.Entries.Select(e => e.Id).ToHashSet();
        var secondPageIds = secondPage.Entries.Select(e => e.Id).ToHashSet();
        firstPageIds.Should().NotIntersectWith(secondPageIds, "pages should not contain duplicate entries");
    }

    /// <summary>
    /// Test-specific response model to handle System.Text.Json deserialization of ActivityFeedDto.
    /// The Data field uses JsonElement since System.Text.Json deserializes Dictionary&lt;string, object&gt;
    /// as JsonElement values.
    /// </summary>
    private record ActivityFeedResponse(
        List<ActivityEntryResponse> Entries,
        DateTime? NextCursor);

    private record ActivityEntryResponse(
        Guid Id,
        Guid BoardId,
        Guid UserId,
        string UserName,
        string EntityType,
        Guid EntityId,
        string ActionType,
        JsonElement Data,
        DateTime OccurredAt);
}
