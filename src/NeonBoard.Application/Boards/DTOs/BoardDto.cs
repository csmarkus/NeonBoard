namespace NeonBoard.Application.Boards.DTOs;

public record BoardDto(
    Guid Id,
    string Name,
    string Slug,
    string Prefix,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ColumnCount);
