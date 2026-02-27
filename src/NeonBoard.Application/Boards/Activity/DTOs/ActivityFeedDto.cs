namespace NeonBoard.Application.Boards.Activity.DTOs;

public record ActivityFeedDto(
    List<ActivityEntryDto> Entries,
    DateTime? NextCursor);
