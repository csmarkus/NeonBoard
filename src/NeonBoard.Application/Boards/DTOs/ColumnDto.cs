namespace NeonBoard.Application.Boards.DTOs;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    Guid BoardId);
