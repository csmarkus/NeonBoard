using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class CorrelationIdTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CorrelationIdTests(NeonBoardWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Request_WithoutCorrelationId_ReturnsGeneratedCorrelationId()
    {
        var response = await _client.GetAsync("/api/projects");

        response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-Id").First();
        correlationId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Request_WithCorrelationId_EchoesItBack()
    {
        var expected = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Add("X-Correlation-Id", expected);

        var response = await _client.SendAsync(request);

        var actual = response.Headers.GetValues("X-Correlation-Id").First();
        actual.Should().Be(expected);
    }
}
