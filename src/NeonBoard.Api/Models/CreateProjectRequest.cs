namespace NeonBoard.Api.Models;

public record CreateProjectRequest(
    string Name,
    string Description,
    Guid OwnerId);
