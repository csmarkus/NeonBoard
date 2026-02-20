namespace NeonBoard.Api.Models;

public record CreateBoardRequest(string Name, string? Prefix = null);
