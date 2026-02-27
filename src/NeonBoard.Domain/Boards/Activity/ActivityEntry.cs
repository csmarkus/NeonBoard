namespace NeonBoard.Domain.Boards.Activity;

public class ActivityEntry
{
    public Guid Id { get; private init; }
    public Guid BoardId { get; private init; }
    public Guid UserId { get; private init; }
    public string UserName { get; private init; } = string.Empty;
    public ActivityEntityType EntityType { get; private init; }
    public Guid EntityId { get; private init; }
    public ActivityActionType ActionType { get; private init; }
    public Dictionary<string, object> Data { get; private init; } = new();
    public DateTime OccurredAt { get; private init; }

    private ActivityEntry() { }

    public static ActivityEntry Create(
        Guid boardId,
        Guid userId,
        string userName,
        ActivityEntityType entityType,
        Guid entityId,
        ActivityActionType actionType,
        Dictionary<string, object> data)
    {
        return new ActivityEntry
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            UserName = userName,
            EntityType = entityType,
            EntityId = entityId,
            ActionType = actionType,
            Data = data,
            OccurredAt = DateTime.UtcNow
        };
    }
}
