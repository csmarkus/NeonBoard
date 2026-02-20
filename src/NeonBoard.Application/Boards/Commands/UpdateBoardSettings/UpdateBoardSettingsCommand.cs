using MediatR;
using NeonBoard.Application.Boards.DTOs;

namespace NeonBoard.Application.Boards.Commands.UpdateBoardSettings;

public record UpdateBoardSettingsCommand(Guid ProjectId, Guid BoardId, string Name, string? Prefix = null) : IRequest<BoardDto>;
