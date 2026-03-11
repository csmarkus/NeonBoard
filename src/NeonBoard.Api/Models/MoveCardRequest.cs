namespace NeonBoard.Api.Models;

public record MoveCardRequest(
    Guid TargetColumnId,
    string NewPosition);
