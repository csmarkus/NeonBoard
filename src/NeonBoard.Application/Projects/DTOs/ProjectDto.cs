namespace NeonBoard.Application.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    string ShortId,
    string Name,
    string Description,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
