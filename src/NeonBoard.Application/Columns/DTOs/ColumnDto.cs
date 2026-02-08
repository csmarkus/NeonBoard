namespace NeonBoard.Application.Columns.DTOs;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    Guid BoardId);
