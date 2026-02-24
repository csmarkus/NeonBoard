using NeonBoard.Application.Labels.DTOs;
using NeonBoard.Domain.Boards.Entities;

namespace NeonBoard.Application.Cards.DTOs;

public record CardDto(
    Guid Id,
    int CardNumber,
    string DisplayId,
    string Title,
    string Description,
    Guid ColumnId,
    int Position,
    List<LabelDto> Labels,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ArchivedAt)
{
    public static CardDto FromCard(Card card, string prefix, List<LabelDto> boardLabels) =>
        new(
            card.Id,
            card.CardNumber,
            $"{prefix}-{card.CardNumber}",
            card.Content.Title,
            card.Content.Description,
            card.ColumnId,
            card.Position.Value,
            card.LabelIds
                .Select(labelId => boardLabels.FirstOrDefault(l => l.Id == labelId))
                .Where(label => label != null)
                .Cast<LabelDto>()
                .OrderBy(label => label.Name)
                .ToList(),
            card.CreatedAt,
            card.UpdatedAt,
            card.ArchivedAt);
}
