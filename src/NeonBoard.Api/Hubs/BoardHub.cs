using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Api.Hubs;

[Authorize]
public class BoardHub : Hub
{
    private static readonly ConcurrentDictionary<string, ConnectionInfo> Connections = new();

    private readonly IProjectAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBoardRepository _boardRepository;

    public BoardHub(
        IProjectAuthorizationService authService,
        ICurrentUserService currentUserService,
        IBoardRepository boardRepository)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _boardRepository = boardRepository;
    }

    public async Task JoinBoard(Guid boardId)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        if (userId == null)
            throw new HubException("User not authenticated.");

        var board = await _boardRepository.GetBoardWithDetailsAsync(boardId);
        if (board == null)
            throw new HubException("Board not found.");

        var hasAccess = await _authService.HasRoleAsync(
            board.ProjectId, userId.Value, ProjectRole.Viewer);
        if (!hasAccess)
            throw new HubException("Access denied.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project:{board.ProjectId}");

        Connections[Context.ConnectionId] = new ConnectionInfo(userId.Value, boardId, board.ProjectId);
    }

    public async Task LeaveBoard(Guid boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board:{boardId}");

        if (Connections.TryGetValue(Context.ConnectionId, out var info))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project:{info.ProjectId}");
            Connections.TryRemove(Context.ConnectionId, out _);
        }
    }

    public async Task JoinProject(Guid projectId)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        if (userId == null)
            throw new HubException("User not authenticated.");

        var hasAccess = await _authService.HasRoleAsync(
            projectId, userId.Value, ProjectRole.Viewer);
        if (!hasAccess)
            throw new HubException("Access denied.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"project:{projectId}");
    }

    public async Task LeaveProject(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project:{projectId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Connections.TryRemove(Context.ConnectionId, out var info))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board:{info.BoardId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project:{info.ProjectId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private record ConnectionInfo(Guid UserId, Guid BoardId, Guid ProjectId);
}
