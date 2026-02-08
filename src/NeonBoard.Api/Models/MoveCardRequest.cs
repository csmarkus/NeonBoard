namespace NeonBoard.Api.Models;

public record MoveCardRequest(
    Guid TargetColumnId,
    int TargetPosition);
