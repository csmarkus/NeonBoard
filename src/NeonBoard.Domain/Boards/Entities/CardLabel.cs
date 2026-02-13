namespace NeonBoard.Domain.Boards.Entities;

public sealed class CardLabel
{
    public Guid CardId { get; private set; }

    public Guid LabelId { get; private set; }

    private CardLabel()
    {
    }

    internal static CardLabel Create(Guid cardId, Guid labelId)
    {
        return new CardLabel
        {
            CardId = cardId,
            LabelId = labelId
        };
    }
}
