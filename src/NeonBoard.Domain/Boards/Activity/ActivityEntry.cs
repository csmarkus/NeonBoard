namespace NeonBoard.Domain.Boards.Activity;

public class ActivityEntry
{
    public Guid Id { get; private init; }
    public Guid BoardId { get; private init; }
    public ActivityEntityType EntityType { get; private init; }
    public Guid EntityId { get; private init; }
    public ActivityActionType ActionType { get; private init; }
    public Dictionary<string, object> Data { get; private init; } = new();
    public DateTime OccurredAt { get; private init; }

    private ActivityEntry() { }

    public static ActivityEntry Create(
        Guid boardId,
        ActivityEntityType entityType,
        Guid entityId,
        ActivityActionType actionType,
        Dictionary<string, object> data)
    {
        return new ActivityEntry
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            EntityType = entityType,
            EntityId = entityId,
            ActionType = actionType,
            Data = data,
            OccurredAt = DateTime.UtcNow
        };
    }
}
