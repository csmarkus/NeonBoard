using System.Collections.Concurrent;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NeonBoard.Application.Cards.Commands.AddCard;
using NeonBoard.Application.Cards.Commands.MoveCard;
using NeonBoard.Application.Columns.Commands.AddColumn;
using NeonBoard.Application.Columns.Commands.DeleteColumn;
using NeonBoard.Application.Columns.Commands.MoveColumn;
using NeonBoard.Application.Columns.Commands.RenameColumn;
using NeonBoard.Application.Columns.Commands.ReorderColumns;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Common;
using NeonBoard.Domain.Projects;

namespace NeonBoard.Api.Hubs;

[Authorize]
public class BoardHub : Hub
{
    private static readonly ConcurrentDictionary<string, ConnectionInfo> Connections = new();

    private readonly IProjectAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBoardRepository _boardRepository;
    private readonly ISender _sender;

    public BoardHub(
        IProjectAuthorizationService authService,
        ICurrentUserService currentUserService,
        IBoardRepository boardRepository,
        ISender sender)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _boardRepository = boardRepository;
        _sender = sender;
    }

    public async Task<Guid> JoinBoard(Guid boardId)
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

        return userId.Value;
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

    public async Task MoveCard(Guid cardId, Guid targetColumnId, string newPosition)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new MoveCardCommand(
                conn.ProjectId, conn.BoardId, cardId, targetColumnId, newPosition));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task AddCard(Guid columnId, string title, string description)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new AddCardCommand(
                conn.ProjectId, conn.BoardId, columnId, title, description));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task AddColumn(string name)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new AddColumnCommand(conn.ProjectId, conn.BoardId, name));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task RenameColumn(Guid columnId, string newName)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new RenameColumnCommand(
                conn.ProjectId, conn.BoardId, columnId, newName));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task MoveColumn(Guid columnId, string newPosition)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new MoveColumnCommand(
                conn.ProjectId, conn.BoardId, columnId, newPosition));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task DeleteColumn(Guid columnId)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new DeleteColumnCommand(
                conn.ProjectId, conn.BoardId, columnId));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task ReorderColumns(List<Guid> columnIds)
    {
        var conn = GetConnectionInfoOrThrow();
        await EnsureEditorRoleAsync(conn);

        try
        {
            await _sender.Send(new ReorderColumnsCommand(
                conn.ProjectId, conn.BoardId, columnIds));
        }
        catch (Exception ex) when (ex is Application.Common.Exceptions.ValidationException
            or Application.Common.Exceptions.NotFoundException
            or DomainException
            or ValidationException)
        {
            throw new HubException(ex.Message);
        }
    }

    private ConnectionInfo GetConnectionInfoOrThrow()
    {
        if (!Connections.TryGetValue(Context.ConnectionId, out var info))
            throw new HubException("Not connected to a board. Call JoinBoard first.");

        return info;
    }

    private async Task EnsureEditorRoleAsync(ConnectionInfo conn)
    {
        var hasRole = await _authService.HasRoleAsync(
            conn.ProjectId, conn.UserId, ProjectRole.Editor);
        if (!hasRole)
            throw new HubException("You do not have permission to perform this action.");
    }

    private record ConnectionInfo(Guid UserId, Guid BoardId, Guid ProjectId);
}
