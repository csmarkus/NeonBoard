namespace NeonBoard.Application.Boards.Activity.DTOs;

public record ActivityEntryDto(
    Guid Id,
    Guid BoardId,
    Guid UserId,
    string UserName,
    string EntityType,
    Guid EntityId,
    string ActionType,
    Dictionary<string, object> Data,
    DateTime OccurredAt);
