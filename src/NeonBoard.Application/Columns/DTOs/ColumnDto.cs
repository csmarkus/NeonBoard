namespace NeonBoard.Application.Columns.DTOs;

public record ColumnDto(
    Guid Id,
    string Name,
    string Position,
    Guid BoardId);
