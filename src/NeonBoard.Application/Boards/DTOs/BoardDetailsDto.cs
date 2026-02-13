using NeonBoard.Application.Columns.DTOs;
using NeonBoard.Application.Cards.DTOs;
using NeonBoard.Application.Labels.DTOs;

namespace NeonBoard.Application.Boards.DTOs;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ColumnDto> Columns,
    List<CardDto> Cards,
    List<LabelDto> Labels);
