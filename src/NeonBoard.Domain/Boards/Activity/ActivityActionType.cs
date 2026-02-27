namespace NeonBoard.Domain.Boards.Activity;

public enum ActivityActionType
{
    Created = 0,
    Updated = 1,
    Moved = 2,
    Deleted = 3,
    Archived = 4,
    Restored = 5,
    Renamed = 6,
    Reordered = 7,
    LabelAdded = 8,
    LabelRemoved = 9,
    PrefixUpdated = 10
}
