using System.Net;
using NeonBoard.IntegrationTests.Infrastructure;

namespace NeonBoard.IntegrationTests.Endpoints;

public class RateLimitingTests : IClassFixture<NeonBoardWebApplicationFactory>
{
    private readonly NeonBoardWebApplicationFactory _factory;

    public RateLimitingTests(NeonBoardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthenticatedRequest_ExceedsRateLimit_Returns429()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimitSettings:AuthenticatedPermitLimit", "3");
            builder.UseSetting("RateLimitSettings:AnonymousPermitLimit", "2");
            builder.UseSetting("RateLimitSettings:WindowInSeconds", "60");
        }).CreateClient();

        // Make requests up to the limit (3)
        for (var i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/api/projects");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // 4th request should be rate limited
        var limitedResponse = await client.GetAsync("/api/projects");
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        limitedResponse.Headers.Should().ContainKey("Retry-After");
    }

    [Fact]
    public async Task RateLimitedResponse_ReturnsProblemDetails()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimitSettings:AuthenticatedPermitLimit", "1");
            builder.UseSetting("RateLimitSettings:WindowInSeconds", "60");
        }).CreateClient();

        // Exhaust the limit
        await client.GetAsync("/api/projects");

        // Next request should return ProblemDetails
        var response = await client.GetAsync("/api/projects");

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Too Many Requests");
        content.Should().Contain("429");
    }
}
