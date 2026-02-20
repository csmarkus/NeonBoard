namespace NeonBoard.Api.Models;

public record UpdateBoardSettingsRequest(string Name, string? Prefix = null);
