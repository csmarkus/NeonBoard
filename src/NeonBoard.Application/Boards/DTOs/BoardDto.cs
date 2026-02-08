namespace NeonBoard.Application.Boards.DTOs;

public record BoardDto(
    Guid Id,
    string Name,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ColumnCount);
