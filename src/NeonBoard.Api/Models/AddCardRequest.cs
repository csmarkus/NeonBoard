namespace NeonBoard.Api.Models;

public record AddCardRequest(
    Guid ColumnId,
    string Title,
    string? Description);
