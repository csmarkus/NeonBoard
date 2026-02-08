namespace NeonBoard.Application.Cards.DTOs;

public record CardDto(
    Guid Id,
    string Title,
    string Description,
    Guid ColumnId,
    int Position,
    DateTime CreatedAt,
    DateTime UpdatedAt);
