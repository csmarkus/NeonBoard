using NeonBoard.Application.Labels.DTOs;

namespace NeonBoard.Application.Cards.DTOs;

public record CardDto(
    Guid Id,
    string Title,
    string Description,
    Guid ColumnId,
    int Position,
    List<LabelDto> Labels,
    DateTime CreatedAt,
    DateTime UpdatedAt);
