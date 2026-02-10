namespace NeonBoard.Api.Constants;

public static class Auth0Claims
{
    private const string Namespace = "https://nb.dev.neonboard.app";

    public const string Email = $"{Namespace}/email";
    public const string Name = $"{Namespace}/name";
}
