namespace NeonBoard.Api.Models;

public record UpdateCardRequest(
    string Title,
    string? Description);
