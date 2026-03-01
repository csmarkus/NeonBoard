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
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", expected);

        var response = await _client.GetAsync("/api/projects");

        var actual = response.Headers.GetValues("X-Correlation-Id").First();
        actual.Should().Be(expected);

        _client.DefaultRequestHeaders.Remove("X-Correlation-Id");
    }
}
