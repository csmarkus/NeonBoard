namespace NeonBoard.Api.Configuration;

public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimitSettings";

    public int AuthenticatedPermitLimit { get; init; } = 100;
    public int AnonymousPermitLimit { get; init; } = 20;
    public int WindowInSeconds { get; init; } = 60;
}
